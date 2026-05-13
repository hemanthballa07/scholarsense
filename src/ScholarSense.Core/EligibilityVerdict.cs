namespace ScholarSense.Core;

public enum Verdict
{
    Eligible,
    Ineligible,
    Unclear
}

public record EligibilityVerdict(
    Verdict Verdict,
    double Confidence,
    IReadOnlyList<string> MatchedCriteria,
    IReadOnlyList<string> FailedCriteria,
    IReadOnlyList<string> UnclearCriteria,
    string? Reason = null);
