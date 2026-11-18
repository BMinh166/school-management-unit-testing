using SchoolManagement.Domain.Students.Enums;
using SchoolManagement.Domain.Students.Entities;
using SchoolManagement.Domain.Students.ValueObjects;
using SchoolManagement.Domain.Students.Services;
using Xunit;

namespace SchoolManagement.Tests.Domain;

public class StudentTests
{

    [Fact]
    public void Constructor_WhenValidDataHasNoId_ShouldCreateActiveStudent()
    {
        // Arrange
        var studentCode = new StudentCode("STU379735");
        var fullName = "Pham Tuan Trinh";
        var email = new Email("abc@gmail.com");
        var academicRecord = new AcademicRecord(6.9m, 91, false);

        // Act
        var student = new Student(studentCode, fullName, email, academicRecord);

        // Assert
        Assert.NotEqual(Guid.Empty, student.Id);
        Assert.Equal(studentCode, student.StudentCode);
        Assert.Equal(fullName, student.FullName);
        Assert.Equal(email, student.Email);
        Assert.Equal(academicRecord, student.AcademicRecord);
        Assert.Equal(StudentStatus.Active, student.Status);
    }

    [Fact]
    public void Constructor_WhenDataIsValid_ShouldCreateActiveStudent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var studentCode = new StudentCode("STU379735");
        var fullName = "Pham Tuan Trinh";
        var email = new Email("abc@gmail.com");
        var academicRecord = new AcademicRecord(6.9m, 91, false);

        // Act
        var student = new Student(id, studentCode, fullName, email, academicRecord);

        // Assert
        Assert.Equal(id, student.Id);
        Assert.Equal(studentCode, student.StudentCode);
        Assert.Equal(fullName, student.FullName);
        Assert.Equal(email, student.Email);
        Assert.Equal(academicRecord, student.AcademicRecord);
        Assert.Equal(StudentStatus.Active, student.Status);
    }

    [Fact]
    public void Constructor_WhenStudentCodeIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fullName = "Pham Tuan Trinh";
        var email = new Email("abc@gmail.com");
        var academicRecord = new AcademicRecord(6.9m, 91, false);


        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Student(id, null!, fullName, email, academicRecord));
    }

    [Fact]
    public void Constructor_WhenIdIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var id = Guid.Empty;
        var studentCode = new StudentCode("STU379735");
        var fullName = "Pham Tuan Trinh";
        var email = new Email("abc@gmail.com");
        var academicRecord = new AcademicRecord(6.9m, 91, false);


        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Student(id, studentCode, fullName, email, academicRecord));
    }

    [Fact]
    public void Constructor_WhenFullNameIsNull_ShouldThrowArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var studentCode = new StudentCode("STU379735");
        var email = new Email("abc@gmail.com");
        var academicRecord = new AcademicRecord(6.9m, 91, false);


        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Student(id, studentCode, null!, email, academicRecord));
    }

    [Fact]
    public void Constructor_WhenFullNameIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var studentCode = new StudentCode("STU379735");
        var fullName = "";
        var email = new Email("abc@gmail.com");
        var academicRecord = new AcademicRecord(6.9m, 91, false);


        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Student(id, studentCode, fullName, email, academicRecord));
    }

    [Fact]
    public void Constructor_WhenEmailIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var studentCode = new StudentCode("STU379735");
        var fullName = "Pham Tuan Trinh";
        var academicRecord = new AcademicRecord(6.9m, 91, false);


        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Student(id, studentCode, fullName, null!, academicRecord));
    }

    [Fact]
    public void Constructor_WhenAcademicRecordIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var studentCode = new StudentCode("STU379735");
        var fullName = "Pham Tuan Trinh";
        var email = new Email("abc@gmail.com");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Student(id, studentCode, fullName, email, null!));
    }

    [Fact]
    public void Deactivate_WhenStudentIsActive_ShouldSetStatusToInactive()
    {
        // Arrange
        var id = Guid.NewGuid();
        var studentCode = new StudentCode("STU379735");
        var fullName = "Pham Tuan Trinh";
        var email = new Email("abc@gmail.com");
        var academicRecord = new AcademicRecord(6.9m, 91, false);
        var student = new Student(id, studentCode, fullName, email, academicRecord);

        // Act
        student.Deactivate();

        // Assert
        Assert.Equal(StudentStatus.Inactive, student.Status);
    }

    [Fact]
    public void Activate_WhenStudentIsInactive_ShouldSetStatusToActive()
    {
        // Arrange
        var id = Guid.NewGuid();
        var studentCode = new StudentCode("STU379735");
        var fullName = "Pham Tuan Trinh";
        var email = new Email("abc@gmail.com");
        var academicRecord = new AcademicRecord(6.9m, 91, false);
        var student = new Student(id, studentCode, fullName, email, academicRecord);

        student.Deactivate();
        Assert.Equal(StudentStatus.Inactive, student.Status);

        // Act
        student.Activate();

        // Assert
        Assert.Equal(StudentStatus.Active, student.Status);
    }

    private static Student CreateStudent(decimal gpa = 5, int credits = 120, bool discipline = false)
    {
        var studentCode = new StudentCode("STU000001");
        var fullName = " Name ";
        var email = new Email("A@EXAMPLE.COM");
        var academicRecord = new AcademicRecord(gpa, credits, discipline);

        return new Student(studentCode, fullName, email, academicRecord);
    }

    [Fact]
    public void Activate_WhenStudentIsActive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var student = CreateStudent();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => student.Activate());
    }

    [Fact]
    public void Deactivate_WhenStudentIsInactive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var student = CreateStudent();
        student.Deactivate();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => student.Deactivate());
    }

    [Fact]
    public void UpdateFullName_WhenStudentIsInactive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var student = CreateStudent();
        student.Deactivate();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => student.UpdateFullName("Other"));
    }

    [Fact]
    public void UpdateEmail_WhenStudentIsInactive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var student = CreateStudent();
        student.Deactivate();
        var email = new Email("b@example.com");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => student.UpdateEmail(email));
    }

    [Fact]
    public void UpdateAcademicRecord_WhenStudentIsInactive_ShouldAllowReactivationAndGraduation()
    {
        // Arrange
        var student = CreateStudent();
        student.Deactivate();
        var academicRecord = new AcademicRecord(6, 130, false);
        student.UpdateAcademicRecord(academicRecord);
        student.Activate();
        student.UpdateFullName(" Other ");

        // Act
        student.Graduate();

        // Assert
        Assert.Equal(StudentStatus.Graduated, student.Status);
    }

    [Fact]
    public void UpdateFullName_WhenReactivatedStudentNameHasWhitespace_ShouldTrimName()
    {
        // Arrange
        var student = CreateStudent();
        student.Deactivate();
        var academicRecord = new AcademicRecord(6, 130, false);
        student.UpdateAcademicRecord(academicRecord);
        student.Activate();

        // Act
        student.UpdateFullName(" Other ");

        // Assert
        Assert.Equal("Other", student.FullName);
    }

    [Fact]
    public void Activate_WhenStudentIsGraduated_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var student = CreateStudent();
        student.Graduate();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => student.Activate());
    }

    [Fact]
    public void UpdateAcademicRecord_WhenStudentIsGraduated_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var student = CreateStudent();
        student.Graduate();
        var academicRecord = new AcademicRecord(6, 130, false);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => student.UpdateAcademicRecord(academicRecord));
    }

    [Fact]
    public void Constructor_WhenNameHasWhitespace_ShouldTrimNameAndSetActiveStatus()
    {
        // Act
        var student = CreateStudent();

        // Assert
        Assert.Equal("Name", student.FullName);
        Assert.Equal(StudentStatus.Active, student.Status);
    }

    [Fact]
    public void UpdateFullName_WhenNameIsWhitespace_ShouldThrowArgumentException()
    {
        // Arrange
        var student = CreateStudent();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => student.UpdateFullName(" "));
    }

    [Fact]
    public void UpdateEmail_WhenEmailIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var student = CreateStudent();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => student.UpdateEmail(null!));
    }
}
