using SchoolManagement.Domain.Students.Entities;
using SchoolManagement.Domain.Students.ValueObjects;
using SchoolManagement.Infrastructure.Persistence;

namespace SchoolManagement.Tests.Infrastructure;

public class InMemoryStudentRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_WhenStudentExists_ShouldReturnDetachedCopy()
    {
        // Arrange
        var repository = new InMemoryStudentRepository();
        var student = CreateStudent();
        await repository.AddAsync(student);

        // Act
        var copy = await repository.GetByIdAsync(student.Id);

        // Assert
        Assert.NotSame(student, copy);
        copy!.UpdateFullName("Changed");
        var storedStudent = await repository.GetByIdAsync(student.Id);
        Assert.Equal("Name", storedStudent!.FullName);
    }

    [Fact]
    public async Task UpdateAsync_WhenDetachedCopyChanges_ShouldPersistChanges()
    {
        // Arrange
        var repository = new InMemoryStudentRepository();
        var student = CreateStudent();
        await repository.AddAsync(student);
        var copy = await repository.GetByIdAsync(student.Id);
        copy!.UpdateFullName("Changed");

        // Act
        await repository.UpdateAsync(copy);

        // Assert
        var storedStudent = await repository.GetByCodeAsync(student.StudentCode);
        Assert.Equal("Changed", storedStudent!.FullName);
    }

    [Fact]
    public async Task ExistsByCodeAsync_WhenStudentExists_ShouldReturnTrue()
    {
        // Arrange
        var repository = new InMemoryStudentRepository();
        var student = CreateStudent();
        await repository.AddAsync(student);

        // Act
        var exists = await repository.ExistsByCodeAsync(student.StudentCode);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task AddAsync_WhenCodeAlreadyExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var repository = new InMemoryStudentRepository();
        var student = CreateStudent();
        var duplicate = CreateStudent();
        await repository.AddAsync(student);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.AddAsync(duplicate));
    }

    [Fact]
    public async Task UpdateAsync_WhenStudentIsMissing_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var repository = new InMemoryStudentRepository();
        var studentCode = new StudentCode("STU000002");
        var fullName = "Name";
        var email = new Email("a@b.com");
        var academicRecord = new AcademicRecord(0, 0, false);
        var student = new Student(studentCode, fullName, email, academicRecord);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.UpdateAsync(student));
    }

    private static Student CreateStudent()
    {
        var studentCode = new StudentCode("STU000001");
        var fullName = " Name ";
        var email = new Email("A@EXAMPLE.COM");
        var academicRecord = new AcademicRecord(5, 120, false);

        return new Student(studentCode, fullName, email, academicRecord);
    }
}
