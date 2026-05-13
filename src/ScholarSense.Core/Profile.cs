namespace ScholarSense.Core;

public record Profile(
    string ProfileId,
    string? Country = null,
    string? State = null,
    decimal? Gpa = null,
    string? DegreeLevel = null,
    string? Major = null,
    IReadOnlyList<string>? DemographicTags = null);
