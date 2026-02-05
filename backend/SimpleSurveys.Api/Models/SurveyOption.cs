namespace SimpleSurveys.Api.Models;

public class SurveyOption
{
    public Guid Id { get; set; }
    public Guid SurveyId { get; set; }
    public OptionType OptionType { get; set; }
    public string? TextValue { get; set; }
    public DateTime? DateValue { get; set; }
    public int DisplayOrder { get; set; }

    public Survey Survey { get; set; } = null!;
    public ICollection<Vote> Votes { get; set; } = [];

    public string DisplayText => OptionType switch
    {
        OptionType.Text => TextValue ?? string.Empty,
        OptionType.Date => DateValue?.ToString("MMMM d, yyyy 'at' h:mm tt") ?? string.Empty,
        _ => string.Empty
    };
}

public enum OptionType
{
    Text,
    Date
}
