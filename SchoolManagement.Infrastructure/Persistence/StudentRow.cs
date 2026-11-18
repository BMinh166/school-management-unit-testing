using SchoolManagement.Domain.Students.Entities;
using SchoolManagement.Domain.Students.Enums;
using SchoolManagement.Domain.Students.ValueObjects;

namespace SchoolManagement.Infrastructure.Persistence;

// Persistence representation; domain objects remain independent of EF Core.
internal sealed class StudentRow
{
    public Guid Id { get; set; }
    public string StudentCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public StudentStatus Status { get; set; }
    public decimal GradePointAverage { get; set; }
    public int EarnedCredits { get; set; }
    public bool HasActiveDisciplinaryAction { get; set; }

    public static StudentRow FromStudent(Student student)
    {
        return new StudentRow
        {
            Id = student.Id,
            StudentCode = student.StudentCode.Value,
            FullName = student.FullName,
            Email = student.Email.Value,
            Status = student.Status,
            GradePointAverage = student.AcademicRecord.GradePointAverage,
            EarnedCredits = student.AcademicRecord.EarnedCredits,
            HasActiveDisciplinaryAction = student.AcademicRecord.HasActiveDisciplinaryAction
        };
    }

    public Student ToStudent()
    {
        var code = new StudentCode(StudentCode);
        var email = new Email(Email);
        var record = new AcademicRecord(GradePointAverage, EarnedCredits, HasActiveDisciplinaryAction);
        var student = new Student(Id, code, FullName, email, record);
        if (Status == StudentStatus.Inactive)
        {
            student.Deactivate();
        }
        else if (Status == StudentStatus.Graduated)
        {
            student.Graduate();
        }
        else if (Status != StudentStatus.Active)
        {
            throw new InvalidOperationException("Stored student status is invalid.");
        }
        return student;
    }
}
