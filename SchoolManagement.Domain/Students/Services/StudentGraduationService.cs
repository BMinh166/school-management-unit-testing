using SchoolManagement.Domain.Students.Entities;
using SchoolManagement.Domain.Students.Enums;

namespace SchoolManagement.Domain.Students.Services;

public sealed class StudentGraduationService
{
    public GraduationEligibility Evaluate(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);
        var reasons = new List<string>();
        if (student.Status != StudentStatus.Active)
        {
            reasons.Add("Student must be active.");
        }
        if (student.AcademicRecord.GradePointAverage < 5m)
        {
            reasons.Add("GPA must be at least 5.0.");
        }
        if (student.AcademicRecord.EarnedCredits < 120)
        {
            reasons.Add("At least 120 credits are required.");
        }
        if (student.AcademicRecord.HasActiveDisciplinaryAction)
        {
            reasons.Add("Student has an active disciplinary action.");
        }
        return new GraduationEligibility(reasons);
    }
}
