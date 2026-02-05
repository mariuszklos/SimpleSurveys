using SimpleSurveys.Api.DTOs;
using SimpleSurveys.Api.Services;

namespace SimpleSurveys.Api.Endpoints;

public static class PublicEndpoints
{
    private const string VoterTokenCookie = "voter_token";

    public static void MapPublicEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/surveys")
            .WithTags("Public");

        group.MapGet("/{id:guid}", GetSurvey);
        group.MapPost("/{id:guid}/vote", Vote);
        group.MapGet("/{id:guid}/my-votes", GetMyVotes);
    }

    private static async Task<IResult> GetSurvey(
        Guid id,
        SurveyService surveyService,
        HttpContext context)
    {
        var voterToken = GetOrCreateVoterToken(context);
        var survey = await surveyService.GetSurveyAsync(id, voterToken);

        if (survey == null)
            return Results.NotFound(new { error = "Survey not found" });

        return Results.Ok(survey);
    }

    private static async Task<IResult> Vote(
        Guid id,
        VoteRequest request,
        SurveyService surveyService,
        HttpContext context)
    {
        var voterToken = GetOrCreateVoterToken(context);
        var (success, error) = await surveyService.VoteAsync(id, request, voterToken);

        if (!success)
            return Results.BadRequest(new { error });

        return Results.Ok(new { message = "Vote recorded successfully" });
    }

    private static async Task<IResult> GetMyVotes(
        Guid id,
        SurveyService surveyService,
        HttpContext context)
    {
        var voterToken = context.Request.Cookies[VoterTokenCookie];
        if (string.IsNullOrEmpty(voterToken))
            return Results.Ok(new MyVotesResponse([], null));

        var response = await surveyService.GetUserVotesAsync(id, voterToken);
        return Results.Ok(response);
    }

    private static string GetOrCreateVoterToken(HttpContext context)
    {
        var token = context.Request.Cookies[VoterTokenCookie];
        if (string.IsNullOrEmpty(token))
        {
            token = Guid.NewGuid().ToString("N");
            context.Response.Cookies.Append(VoterTokenCookie, token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = true,
                MaxAge = TimeSpan.FromDays(365)
            });
        }
        return token;
    }
}
