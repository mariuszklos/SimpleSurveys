namespace SimpleSurveys.Api.Models;

public class Survey
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public SelectionMode SelectionMode { get; set; }
    public DateTime Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<SurveyOption> Options { get; set; } = [];
    public ICollection<Vote> Votes { get; set; } = [];

    public bool IsActive => DateTime.UtcNow < Deadline;
}

public enum SelectionMode
{
    Single,
    Multiple
}
