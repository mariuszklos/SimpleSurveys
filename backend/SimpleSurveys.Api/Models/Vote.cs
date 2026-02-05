namespace SimpleSurveys.Api.Models;

public class Vote
{
    public Guid Id { get; set; }
    public Guid SurveyId { get; set; }
    public Guid OptionId { get; set; }
    public required string VoterToken { get; set; }
    public string? VoterName { get; set; }
    public DateTime CreatedAt { get; set; }

    public Survey Survey { get; set; } = null!;
    public SurveyOption Option { get; set; } = null!;
}
