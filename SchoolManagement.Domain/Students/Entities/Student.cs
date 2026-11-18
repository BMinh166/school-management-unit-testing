using SchoolManagement.Domain.Students.Enums;
using SchoolManagement.Domain.Students.Services;
using SchoolManagement.Domain.Students.ValueObjects;

namespace SchoolManagement.Domain.Students.Entities;

public sealed class Student
{
    public Guid Id { get; }
    public StudentCode StudentCode { get; }
    public string FullName { get; private set; }
    public Email Email { get; private set; }
    public StudentStatus Status { get; private set; }
    public AcademicRecord AcademicRecord { get; private set; }

    public Student(StudentCode studentCode, string fullName, Email email, AcademicRecord academicRecord)
        : this(Guid.NewGuid(), studentCode, fullName, email, academicRecord) { }

    // Explicit identity also permits deterministic seed data; new students always start active.
    public Student(Guid id, StudentCode studentCode, string fullName, Email email, AcademicRecord academicRecord)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Student ID is required.", nameof(id));
        }
        Id = id;
        StudentCode = studentCode
            ?? throw new ArgumentNullException(nameof(studentCode));
        FullName = ValidateName(fullName);
        Email = email
            ?? throw new ArgumentNullException(nameof(email));
        AcademicRecord = academicRecord
            ?? throw new ArgumentNullException(nameof(academicRecord));
        Status = StudentStatus.Active;
    }

    private Student(Student source)
    {
        Id = source.Id;
        StudentCode = source.StudentCode;
        FullName = source.FullName;
        Email = source.Email;
        Status = source.Status;
        AcademicRecord = source.AcademicRecord;
    }

    // Value objects are immutable, so this detached copy has no shared mutable state.
    public Student Copy()
    {
        return new Student(this);
    }

    public void UpdateFullName(string fullName)
    {
        RequireActive();
        FullName = ValidateName(fullName);
    }
    public void UpdateEmail(Email email)
    {
        RequireActive();
        Email = email ?? throw new ArgumentNullException(nameof(email));
    }
    public void UpdateAcademicRecord(AcademicRecord record)
    {
        if (Status == StudentStatus.Graduated)
        {
            throw new InvalidOperationException("Graduation is a terminal state.");
        }
        AcademicRecord = record ?? throw new ArgumentNullException(nameof(record));
    }
    public void Activate()
    {
        if (Status != StudentStatus.Inactive)
        {
            throw new InvalidOperationException("Only inactive students can be activated.");
        }
        Status = StudentStatus.Active;
    }
    public void Deactivate()
    {
        RequireActive();
        Status = StudentStatus.Inactive;
    }
    public void Graduate()
    {
        var result = new StudentGraduationService().Evaluate(this);
        if (!result.IsEligible)
        {
            throw new InvalidOperationException(string.Join(" ", result.Reasons));
        }
        Status = StudentStatus.Graduated;
    }
    private void RequireActive()
    {
        if (Status != StudentStatus.Active)
        {
            throw new InvalidOperationException("Student must be active.");
        }
    }
    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Student name is required.",
                nameof(name));
        }

        return name.Trim();
    }
}
