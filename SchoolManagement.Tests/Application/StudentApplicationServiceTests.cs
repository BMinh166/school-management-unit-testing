using Moq;
using SchoolManagement.Application.Students.Services;
using SchoolManagement.Domain.Students.Entities;
using SchoolManagement.Domain.Students.Enums;
using SchoolManagement.Domain.Students.Repositories;
using SchoolManagement.Domain.Students.Services;
using SchoolManagement.Domain.Students.ValueObjects;
using SchoolManagement.Infrastructure.Persistence;
using Xunit;

namespace SchoolManagement.Tests.Application;

// These tests use mocks for repository interactions and an in-memory repository for service workflows.
public class StudentApplicationServiceTests
{
    [Fact]
    public async Task CreateStudentAsync_WhenCodeAlreadyExists_ShouldThrowAndNotAddStudent()
    {
        // Arrange
        var repository = new Mock<IStudentRepository>();
        var graduationService = new StudentGraduationService();
        var service = new StudentApplicationService(repository.Object, graduationService);
        repository.Setup(r => r.ExistsByCodeAsync(It.IsAny<StudentCode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateStudentAsync("STU000001", "Name", "a@b.com", 5, 120, false));
        repository.Verify(r => r.AddAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GraduateStudentAsync_WhenStudentIsIneligible_ShouldThrowAndNotUpdateStudent()
    {
        // Arrange
        var repository = new Mock<IStudentRepository>();
        var graduationService = new StudentGraduationService();
        var service = new StudentApplicationService(repository.Object, graduationService);
        var student = CreateStudent(4);
        repository.Setup(r => r.GetByIdAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GraduateStudentAsync(student.Id));
        repository.Verify(r => r.UpdateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateStudentAsync_WhenValuesNeedNormalization_ShouldReturnNormalizedValues()
    {
        // Arrange
        var repository = new InMemoryStudentRepository();
        var graduationService = new StudentGraduationService();
        var service = new StudentApplicationService(repository, graduationService);

        // Act
        var student = await service.CreateStudentAsync("STU123456", " Name ", "A@EXAMPLE.COM", 5, 120, false);

        // Assert
        Assert.Equal("Name", student.FullName);
        Assert.Equal("a@example.com", student.Email);
    }

    [Fact]
    public async Task GetStudentByIdAsync_WhenStudentWasCreated_ShouldReturnCreatedStudent()
    {
        // Arrange
        var repository = new InMemoryStudentRepository();
        var graduationService = new StudentGraduationService();
        var service = new StudentApplicationService(repository, graduationService);
        var createdStudent = await service.CreateStudentAsync("STU123456", " Name ", "A@EXAMPLE.COM", 5, 120, false);

        // Act
        var retrievedStudent = await service.GetStudentByIdAsync(createdStudent.Id);

        // Assert
        Assert.Equal(createdStudent, retrievedStudent);
    }

    [Fact]
    public async Task UpdateStudentNameAsync_WhenStudentExists_ShouldPersistName()
    {
        // Arrange
        var repository = new InMemoryStudentRepository();
        var graduationService = new StudentGraduationService();
        var service = new StudentApplicationService(repository, graduationService);
        var student = await service.CreateStudentAsync("STU123456", " Name ", "A@EXAMPLE.COM", 5, 120, false);

        // Act
        await service.UpdateStudentNameAsync(student.Id, "New Name");

        // Assert
        var result = await service.GetStudentByIdAsync(student.Id);
        Assert.Equal("New Name", result.FullName);
    }

    [Fact]
    public async Task UpdateStudentEmailAsync_WhenStudentExists_ShouldPersistEmail()
    {
        // Arrange
        var repository = new InMemoryStudentRepository();
        var graduationService = new StudentGraduationService();
        var service = new StudentApplicationService(repository, graduationService);
        var student = await service.CreateStudentAsync("STU123456", " Name ", "A@EXAMPLE.COM", 5, 120, false);

        // Act
        await service.UpdateStudentEmailAsync(student.Id, "new@example.com");

        // Assert
        var result = await service.GetStudentByIdAsync(student.Id);
        Assert.Equal("new@example.com", result.Email);
    }

    [Fact]
    public async Task UpdateAcademicRecordAsync_WhenStudentExists_ShouldPersistRecord()
    {
        // Arrange
        var repository = new InMemoryStudentRepository();
        var graduationService = new StudentGraduationService();
        var service = new StudentApplicationService(repository, graduationService);
        var student = await service.CreateStudentAsync("STU123456", " Name ", "A@EXAMPLE.COM", 5, 120, false);

        // Act
        await service.UpdateAcademicRecordAsync(student.Id, 6, 130, false);

        // Assert
        var result = await service.GetStudentByIdAsync(student.Id);
        Assert.Equal(6, result.GradePointAverage);
        Assert.Equal(130, result.EarnedCredits);
    }

    [Fact]
    public async Task EvaluateGraduationAsync_WhenStudentWasDeactivated_ShouldReturnIneligible()
    {
        // Arrange
        var repository = new InMemoryStudentRepository();
        var graduationService = new StudentGraduationService();
        var service = new StudentApplicationService(repository, graduationService);
        var student = await service.CreateStudentAsync("STU123456", " Name ", "A@EXAMPLE.COM", 5, 120, false);
        await service.UpdateAcademicRecordAsync(student.Id, 6, 130, false);
        await service.DeactivateStudentAsync(student.Id);

        // Act
        var eligibility = await service.EvaluateGraduationAsync(student.Id);

        // Assert
        Assert.False(eligibility.IsEligible);
    }

    [Fact]
    public async Task EvaluateGraduationAsync_WhenStudentWasReactivated_ShouldReturnEligible()
    {
        // Arrange
        var repository = new InMemoryStudentRepository();
        var graduationService = new StudentGraduationService();
        var service = new StudentApplicationService(repository, graduationService);
        var student = await service.CreateStudentAsync("STU123456", " Name ", "A@EXAMPLE.COM", 5, 120, false);
        await service.UpdateAcademicRecordAsync(student.Id, 6, 130, false);
        await service.DeactivateStudentAsync(student.Id);
        await service.ActivateStudentAsync(student.Id);

        // Act
        var eligibility = await service.EvaluateGraduationAsync(student.Id);

        // Assert
        Assert.True(eligibility.IsEligible);
    }

    [Fact]
    public async Task GraduateStudentAsync_WhenStudentIsEligible_ShouldPersistGraduatedStatus()
    {
        // Arrange
        var repository = new InMemoryStudentRepository();
        var graduationService = new StudentGraduationService();
        var service = new StudentApplicationService(repository, graduationService);
        var student = await service.CreateStudentAsync("STU123456", " Name ", "A@EXAMPLE.COM", 5, 120, false);
        await service.UpdateStudentNameAsync(student.Id, "New Name");
        await service.UpdateStudentEmailAsync(student.Id, "new@example.com");
        await service.UpdateAcademicRecordAsync(student.Id, 6, 130, false);
        await service.DeactivateStudentAsync(student.Id);
        await service.ActivateStudentAsync(student.Id);

        // Act
        await service.GraduateStudentAsync(student.Id);

        // Assert
        var result = await service.GetStudentByIdAsync(student.Id);
        Assert.Equal("New Name", result.FullName);
        Assert.Equal("new@example.com", result.Email);
        Assert.Equal(6, result.GradePointAverage);
        Assert.Equal(130, result.EarnedCredits);
        Assert.Equal(StudentStatus.Graduated, result.Status);
    }

    [Fact]
    public async Task UpdateStudentNameAsync_WhenStudentIsInactive_ShouldThrowAndLeaveStoredNameUnchanged()
    {
        // Arrange
        var repository = new InMemoryStudentRepository();
        var graduationService = new StudentGraduationService();
        var service = new StudentApplicationService(repository, graduationService);
        var student = await service.CreateStudentAsync("STU123456", "Name", "a@example.com", 5, 120, false);
        await service.DeactivateStudentAsync(student.Id);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateStudentNameAsync(student.Id, "Changed"));

        // Assert
        var storedStudent = await service.GetStudentByIdAsync(student.Id);
        Assert.Equal("Name", storedStudent.FullName);
        Assert.Equal(StudentStatus.Inactive, storedStudent.Status);
    }

    [Fact]
    public async Task GetStudentByIdAsync_WhenStudentIsMissing_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var repository = new Mock<IStudentRepository>();
        var graduationService = new StudentGraduationService();
        var service = new StudentApplicationService(repository.Object, graduationService);
        var id = Guid.NewGuid();
        repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Student?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetStudentByIdAsync(id));
    }

    [Fact]
    public async Task GetStudentByIdAsync_WhenRepositoryFails_ShouldPropagateException()
    {
        // Arrange
        var repository = new Mock<IStudentRepository>();
        var graduationService = new StudentGraduationService();
        var service = new StudentApplicationService(repository.Object, graduationService);
        var id = Guid.NewGuid();
        var failure = new InvalidOperationException("repository failure");
        repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ThrowsAsync(failure);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetStudentByIdAsync(id));

        // Assert
        Assert.Same(failure, exception);
    }

    [Fact]
    public async Task GetStudentByIdAsync_WhenCancellationIsRequested_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var repository = new InMemoryStudentRepository();
        var graduationService = new StudentGraduationService();
        var service = new StudentApplicationService(repository, graduationService);
        var id = Guid.NewGuid();
        using var source = new CancellationTokenSource();
        source.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetStudentByIdAsync(id, source.Token));
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
