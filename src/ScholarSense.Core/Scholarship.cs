namespace ScholarSense.Core;

public record Scholarship(
    string Id,
    string Title,
    string Description,
    string EligibilityText,
    string Sponsor,
    decimal AmountUsd,
    DateOnly Deadline,
    decimal? MinGpa,
    IReadOnlyList<string> EligibleCountries,
    IReadOnlyList<string> EligibleStates,
    IReadOnlyList<string> EligibleMajors,
    IReadOnlyList<string> DemographicTags,
    string SourceUrl);
