using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SchoolManagement.API.Students;
using SchoolManagement.Application.Students.DTOs;
using SchoolManagement.Application.Students.Services;
using SchoolManagement.Domain.Students.Entities;
using SchoolManagement.Domain.Students.Repositories;
using SchoolManagement.Domain.Students.Services;
using SchoolManagement.Domain.Students.ValueObjects;
using SchoolManagement.Infrastructure.Persistence;

namespace SchoolManagement.Tests.API;

public class StudentsControllerTests
{
    [Fact]
    public async Task Build_WhenConfigured_ShouldUseScopedPostgresRepository()
    {
        // Arrange
        await using var app = ApiHost.Build(new[] { "--ConnectionStrings:SchoolDatabase=Host=localhost;Database=school_management" });
        using var firstScope = app.Services.CreateScope();
        using var secondScope = app.Services.CreateScope();

        // Act
        var first = firstScope.ServiceProvider.GetRequiredService<IStudentRepository>();
        var sameScope = firstScope.ServiceProvider.GetRequiredService<IStudentRepository>();
        var second = secondScope.ServiceProvider.GetRequiredService<IStudentRepository>();

        // Assert
        Assert.IsType<PostgresStudentRepository>(first);
        Assert.Same(first, sameScope);
        Assert.NotSame(first, second);
    }

    [Fact]
    public async Task GetById_WhenStudentExists_ShouldReturnStudent()
    {
        // Arrange
        var repository = new Mock<IStudentRepository>();
        var student = new Student(new StudentCode("STU123456"), "Name", new Email("a@example.com"), new AcademicRecord(5, 120, false));
        repository.Setup(item => item.GetByIdAsync(student.Id, It.IsAny<CancellationToken>())).ReturnsAsync(student);
        var service = new StudentApplicationService(repository.Object, new StudentGraduationService());
        var controller = new StudentsController(service);

        // Act
        var response = await controller.GetById(student.Id, CancellationToken.None);

        // Assert
        var result = Assert.IsType<OkObjectResult>(response.Result);
        var value = Assert.IsType<StudentDto>(result.Value);
        Assert.Equal(student.Id, value.Id);
    }

    [Fact]
    public async Task Create_WhenRequestIsValid_ShouldAddStudentAndReturnCreatedResult()
    {
        // Arrange
        var repository = new Mock<IStudentRepository>();
        var service = new StudentApplicationService(repository.Object, new StudentGraduationService());
        var controller = new StudentsController(service);
        var request = new CreateStudentRequest
        {
            StudentCode = "STU123456",
            FullName = "Name",
            Email = "a@example.com",
            GradePointAverage = 5,
            EarnedCredits = 120,
            HasActiveDisciplinaryAction = false
        };

        // Act
        var response = await controller.Create(request, CancellationToken.None);

        // Assert
        Assert.IsType<CreatedAtActionResult>(response.Result);
        repository.Verify(item => item.AddAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
