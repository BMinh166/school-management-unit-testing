namespace SchoolManagement.Domain.Students.ValueObjects;

public sealed record AcademicRecord
{
    public decimal GradePointAverage { get; }
    public int EarnedCredits { get; }
    public bool HasActiveDisciplinaryAction { get; }

    public AcademicRecord(decimal gradePointAverage, int earnedCredits, bool hasActiveDisciplinaryAction)
    {
        if (gradePointAverage is < 0 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(gradePointAverage), "GPA must be between 0 and 10.");
        }
        if (earnedCredits < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(earnedCredits), "Credits cannot be negative.");
        }
        GradePointAverage = gradePointAverage;
        EarnedCredits = earnedCredits;
        HasActiveDisciplinaryAction = hasActiveDisciplinaryAction;
    }
}
