using SchoolManagement.Domain.Students.Entities;
using SchoolManagement.Domain.Students.Enums;
using SchoolManagement.Domain.Students.Services;
using SchoolManagement.Domain.Students.ValueObjects;
using Xunit;

namespace SchoolManagement.Tests.Domain;

public class StudentGraduationServiceTests
{
    [Fact]
    public void Evaluate_WhenMultipleConditionsFail_ShouldReturnAllReasonsInOrder()
    {
        // Arrange
        var student = CreateStudent(4, 119, true);
        student.Deactivate();
        var service = new StudentGraduationService();
        var expectedReasons = new[]
        {
            "Student must be active.",
            "GPA must be at least 5.0.",
            "At least 120 credits are required.",
            "Student has an active disciplinary action."
        };

        // Act
        var result = service.Evaluate(student);

        // Assert
        Assert.False(result.IsEligible);
        Assert.Equal(expectedReasons, result.Reasons);
    }

    [Fact]
    public void Graduate_WhenInactiveStudentFailsEligibility_ShouldThrowAndRemainInactive()
    {
        // Arrange
        var student = CreateStudent(4, 119, true);
        student.Deactivate();

        // Act
        Assert.Throws<InvalidOperationException>(() => student.Graduate());

        // Assert
        Assert.Equal(StudentStatus.Inactive, student.Status);
    }

    [Fact]
    public void Evaluate_WhenStudentMeetsMinimumRequirements_ShouldReturnEligible()
    {
        // Arrange
        var student = CreateStudent();
        var service = new StudentGraduationService();

        // Act
        var result = service.Evaluate(student);

        // Assert
        Assert.True(result.IsEligible);
    }

    private static Student CreateStudent(decimal gpa = 5, int credits = 120, bool discipline = false)
    {
        var studentCode = new StudentCode("STU000001");
        var fullName = " Name ";
        var email = new Email("A@EXAMPLE.COM");
        var academicRecord = new AcademicRecord(gpa, credits, discipline);

        return new Student(studentCode, fullName, email, academicRecord);
    }
}
