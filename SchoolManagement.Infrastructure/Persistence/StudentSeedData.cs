using SchoolManagement.Domain.Students.Entities;
using SchoolManagement.Domain.Students.ValueObjects;

namespace SchoolManagement.Infrastructure.Persistence;

public static class StudentSeedData
{
    public static IReadOnlyList<Student> Create()
    {
        var eligible = CreateStudent("00000000-0000-0000-0000-000000000001", "STU000001", "An Nguyen", "an@example.com", 7m, 130);
        var ineligible = CreateStudent("00000000-0000-0000-0000-000000000002", "STU000002", "Binh Tran", "binh@example.com", 4m, 100);
        var inactive = CreateStudent("00000000-0000-0000-0000-000000000003", "STU000003", "Chi Le", "chi@example.com", 6m, 120);
        inactive.Deactivate();
        var graduated = CreateStudent("00000000-0000-0000-0000-000000000004", "STU000004", "Dung Pham", "dung@example.com", 8m, 140);
        graduated.Graduate();
        return Array.AsReadOnly(new[] { eligible, ineligible, inactive, graduated });
    }

    private static Student CreateStudent(string id, string code, string name, string email, decimal gpa, int credits)
    {
        var studentId = Guid.Parse(id);
        var studentCode = new StudentCode(code);
        var studentEmail = new Email(email);
        var academicRecord = new AcademicRecord(gpa, credits, false);

        return new Student(studentId, studentCode, name, studentEmail, academicRecord);
    }
}
