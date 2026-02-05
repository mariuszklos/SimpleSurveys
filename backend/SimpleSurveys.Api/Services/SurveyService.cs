using Microsoft.EntityFrameworkCore;
using SimpleSurveys.Api.Data;
using SimpleSurveys.Api.DTOs;
using SimpleSurveys.Api.Models;

namespace SimpleSurveys.Api.Services;

public class SurveyService
{
    private readonly AppDbContext _db;

    public SurveyService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SurveyResponse?> GetSurveyAsync(Guid id, string? voterToken)
    {
        var survey = await _db.Surveys
            .Include(s => s.Options.OrderBy(o => o.DisplayOrder))
            .Include(s => s.Votes)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (survey == null) return null;

        var userVotes = voterToken != null
            ? survey.Votes.Where(v => v.VoterToken == voterToken).ToList()
            : new List<Vote>();

        var currentVoterName = userVotes.FirstOrDefault()?.VoterName;

        var maxVotes = survey.Options.Max(o => o.Votes.Count);

        return new SurveyResponse(
            survey.Id,
            survey.Title,
            survey.Description,
            survey.SelectionMode,
            survey.Deadline,
            survey.IsActive,
            survey.Options.Select(o => new OptionResponse(
                o.Id,
                o.OptionType,
                o.TextValue,
                o.DateValue,
                o.DisplayText,
                o.Votes.Count,
                !survey.IsActive && o.Votes.Count == maxVotes && maxVotes > 0
            )).ToList(),
            survey.Votes.Select(v => v.VoterToken).Distinct().Count(),
            userVotes.Count > 0,
            currentVoterName
        );
    }

    public async Task<MyVotesResponse> GetUserVotesAsync(Guid surveyId, string voterToken)
    {
        var votes = await _db.Votes
            .Where(v => v.SurveyId == surveyId && v.VoterToken == voterToken)
            .ToListAsync();

        return new MyVotesResponse(
            votes.Select(v => v.OptionId).ToList(),
            votes.FirstOrDefault()?.VoterName
        );
    }

    public async Task<(bool Success, string? Error)> VoteAsync(Guid surveyId, VoteRequest request, string voterToken)
    {
        var survey = await _db.Surveys
            .Include(s => s.Options)
            .FirstOrDefaultAsync(s => s.Id == surveyId);

        if (survey == null)
            return (false, "Survey not found");

        if (!survey.IsActive)
            return (false, "Survey is closed");

        if (request.OptionIds.Count == 0)
            return (false, "At least one option must be selected");

        if (survey.SelectionMode == SelectionMode.Single && request.OptionIds.Count > 1)
            return (false, "Only one option can be selected for this survey");

        var validOptionIds = survey.Options.Select(o => o.Id).ToHashSet();
        if (!request.OptionIds.All(id => validOptionIds.Contains(id)))
            return (false, "Invalid option selected");

        // Remove existing votes for this voter
        var existingVotes = await _db.Votes
            .Where(v => v.SurveyId == surveyId && v.VoterToken == voterToken)
            .ToListAsync();
        _db.Votes.RemoveRange(existingVotes);

        // Add new votes
        var now = DateTime.UtcNow;
        var voterName = request.VoterName?.Trim();
        foreach (var optionId in request.OptionIds)
        {
            _db.Votes.Add(new Vote
            {
                Id = Guid.NewGuid(),
                SurveyId = surveyId,
                OptionId = optionId,
                VoterToken = voterToken,
                VoterName = voterName,
                CreatedAt = now
            });
        }

        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<List<SurveyListItemResponse>> GetAllSurveysAsync()
    {
        return await _db.Surveys
            .Include(s => s.Votes)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SurveyListItemResponse(
                s.Id,
                s.Title,
                s.SelectionMode,
                s.Deadline,
                DateTime.UtcNow < s.Deadline,
                s.Votes.Select(v => v.VoterToken).Distinct().Count(),
                s.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<Survey> CreateSurveyAsync(CreateSurveyRequest request)
    {
        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            SelectionMode = request.SelectionMode,
            Deadline = request.Deadline,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        for (int i = 0; i < request.Options.Count; i++)
        {
            var opt = request.Options[i];
            survey.Options.Add(new SurveyOption
            {
                Id = Guid.NewGuid(),
                SurveyId = survey.Id,
                OptionType = opt.OptionType,
                TextValue = opt.TextValue,
                DateValue = opt.DateValue,
                DisplayOrder = i
            });
        }

        _db.Surveys.Add(survey);
        await _db.SaveChangesAsync();
        return survey;
    }

    public async Task<(bool Success, string? Error)> UpdateSurveyAsync(Guid id, UpdateSurveyRequest request)
    {
        var survey = await _db.Surveys
            .Include(s => s.Options)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (survey == null)
            return (false, "Survey not found");

        survey.Title = request.Title;
        survey.Description = request.Description;
        survey.SelectionMode = request.SelectionMode;
        survey.Deadline = request.Deadline;
        survey.UpdatedAt = DateTime.UtcNow;

        // Remove options that are not in the request
        var requestOptionIds = request.Options
            .Where(o => o.Id.HasValue)
            .Select(o => o.Id!.Value)
            .ToHashSet();

        var optionsToRemove = survey.Options
            .Where(o => !requestOptionIds.Contains(o.Id))
            .ToList();
        _db.SurveyOptions.RemoveRange(optionsToRemove);

        // Update existing and add new options
        for (int i = 0; i < request.Options.Count; i++)
        {
            var opt = request.Options[i];
            if (opt.Id.HasValue)
            {
                var existing = survey.Options.FirstOrDefault(o => o.Id == opt.Id.Value);
                if (existing != null)
                {
                    existing.OptionType = opt.OptionType;
                    existing.TextValue = opt.TextValue;
                    existing.DateValue = opt.DateValue;
                    existing.DisplayOrder = i;
                }
            }
            else
            {
                survey.Options.Add(new SurveyOption
                {
                    Id = Guid.NewGuid(),
                    SurveyId = survey.Id,
                    OptionType = opt.OptionType,
                    TextValue = opt.TextValue,
                    DateValue = opt.DateValue,
                    DisplayOrder = i
                });
            }
        }

        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> DeleteSurveyAsync(Guid id)
    {
        var survey = await _db.Surveys.FindAsync(id);
        if (survey == null) return false;

        _db.Surveys.Remove(survey);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<SurveyVotersResponse?> GetSurveyVotersAsync(Guid surveyId)
    {
        var survey = await _db.Surveys
            .Include(s => s.Options)
            .Include(s => s.Votes)
            .FirstOrDefaultAsync(s => s.Id == surveyId);

        if (survey == null) return null;

        var optionLookup = survey.Options.ToDictionary(o => o.Id, o => o.DisplayText);

        var voters = survey.Votes
            .GroupBy(v => v.VoterToken)
            .Select(g => new VoterSummary(
                g.First().VoterName ?? "Anonymous",
                g.Select(v => optionLookup.GetValueOrDefault(v.OptionId, "Unknown")).ToList(),
                g.First().CreatedAt
            ))
            .OrderByDescending(v => v.VotedAt)
            .ToList();

        return new SurveyVotersResponse(surveyId, voters);
    }
}
