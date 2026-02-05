using SimpleSurveys.Api.Models;

namespace SimpleSurveys.Api.DTOs;

// Request DTOs
public record CreateSurveyRequest(
    string Title,
    string? Description,
    SelectionMode SelectionMode,
    DateTime Deadline,
    List<CreateOptionRequest> Options
);

public record UpdateSurveyRequest(
    string Title,
    string? Description,
    SelectionMode SelectionMode,
    DateTime Deadline,
    List<UpdateOptionRequest> Options
);

public record CreateOptionRequest(
    OptionType OptionType,
    string? TextValue,
    DateTime? DateValue
);

public record UpdateOptionRequest(
    Guid? Id,
    OptionType OptionType,
    string? TextValue,
    DateTime? DateValue
);

public record VoteRequest(List<Guid> OptionIds, string? VoterName);

public record AdminLoginRequest(string Password);

// Response DTOs
public record SurveyResponse(
    Guid Id,
    string Title,
    string? Description,
    SelectionMode SelectionMode,
    DateTime Deadline,
    bool IsActive,
    List<OptionResponse> Options,
    int TotalVotes,
    bool UserHasVoted,
    string? CurrentVoterName
);

public record OptionResponse(
    Guid Id,
    OptionType OptionType,
    string? TextValue,
    DateTime? DateValue,
    string DisplayText,
    int VoteCount,
    bool IsWinner
);

public record SurveyListItemResponse(
    Guid Id,
    string Title,
    SelectionMode SelectionMode,
    DateTime Deadline,
    bool IsActive,
    int TotalVotes,
    DateTime CreatedAt
);

public record MyVotesResponse(List<Guid> OptionIds, string? VoterName);

public record VoterSummary(
    string VoterName,
    List<string> SelectedOptions,
    DateTime VotedAt
);

public record SurveyVotersResponse(
    Guid SurveyId,
    List<VoterSummary> Voters
);
