namespace SchoolManagement.Domain.Students.Services;

public sealed class GraduationEligibility
{
    private readonly IReadOnlyList<string> _reasons;
    public bool IsEligible => _reasons.Count == 0;
    public IReadOnlyList<string> Reasons => _reasons;

    internal GraduationEligibility(IEnumerable<string> reasons)
    {
        _reasons = Array.AsReadOnly(reasons.ToArray());
    }
}
