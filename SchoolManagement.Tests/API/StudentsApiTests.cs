using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SchoolManagement.API.Students;
using SchoolManagement.Application.Students.DTOs;
using SchoolManagement.Domain.Students.Enums;
using SchoolManagement.Domain.Students.Repositories;
using SchoolManagement.Infrastructure.Persistence;

namespace SchoolManagement.Tests.API;

public class StudentsApiTests : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _app = ApiHost.Build(new[] { "--urls", "http://127.0.0.1:0" }, services =>
        {
            services.RemoveAll<IStudentRepository>();
            services.AddSingleton<IStudentRepository, InMemoryStudentRepository>();
        });
        await _app.StartAsync();
        var server = _app.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();
        var address = addresses!.Addresses.Single();
        _client = new HttpClient { BaseAddress = new Uri(address) };
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task Create_WhenRequestIsValid_ShouldReturnCreatedStudentAndLocation()
    {
        // Arrange
        var request = CreateRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/students", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<StudentDto>();
        Assert.NotNull(created);
        Assert.Equal("STU123456", created.StudentCode);
        Assert.NotNull(response.Headers.Location);
        var retrieved = await _client.GetFromJsonAsync<StudentDto>(response.Headers.Location);
        Assert.Equal(created, retrieved);
    }

    [Fact]
    public async Task Create_WhenCodeIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var request = CreateRequest("invalid");

        // Act
        var response = await _client.PostAsJsonAsync("/api/students", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(400, problem!.Status);
        Assert.False(string.IsNullOrWhiteSpace(problem.Detail));
    }

    [Fact]
    public async Task Create_WhenRequiredFieldsAreMissing_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new { studentCode = "STU123456", fullName = "Name", email = "a@example.com" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/students", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal(400, problem!.Status);
        Assert.NotEmpty(problem.Errors);
    }

    [Fact]
    public async Task Create_WhenJsonIsMalformed_ShouldReturnBadRequest()
    {
        // Arrange
        using var content = new StringContent("{", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/students", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(400, problem!.Status);
    }

    [Fact]
    public async Task GetById_WhenStudentIsMissing_ShouldReturnNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/students/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(404, problem!.Status);
    }

    [Fact]
    public async Task Create_WhenCodeAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        var request = CreateRequest();
        await _client.PostAsJsonAsync("/api/students", request);

        // Act
        var response = await _client.PostAsJsonAsync("/api/students", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(409, problem!.Status);
    }

    [Fact]
    public async Task Activate_WhenStudentIsActive_ShouldReturnConflictAndKeepStatus()
    {
        // Arrange
        var student = await CreateStudentAsync();

        // Act
        var response = await _client.PostAsync($"/api/students/{student.Id}/activate", null);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(409, problem!.Status);
        var stored = await _client.GetFromJsonAsync<StudentDto>($"/api/students/{student.Id}");
        Assert.Equal(StudentStatus.Active, stored!.Status);
    }

    [Fact]
    public async Task Activate_WhenStudentIsInactive_ShouldPersistActiveStatus()
    {
        // Arrange
        var student = await CreateStudentAsync();
        var deactivateResponse = await _client.PostAsync($"/api/students/{student.Id}/deactivate", null);
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);

        // Act
        var response = await _client.PostAsync($"/api/students/{student.Id}/activate", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stored = await _client.GetFromJsonAsync<StudentDto>($"/api/students/{student.Id}");
        Assert.Equal(StudentStatus.Active, stored!.Status);
    }

    [Fact]
    public async Task Graduate_WhenStudentIsInactive_ShouldReturnConflictAndKeepStatus()
    {
        // Arrange
        var student = await CreateStudentAsync();
        await _client.PostAsync($"/api/students/{student.Id}/deactivate", null);

        // Act
        var response = await _client.PostAsync($"/api/students/{student.Id}/graduate", null);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var stored = await _client.GetFromJsonAsync<StudentDto>($"/api/students/{student.Id}");
        Assert.Equal(StudentStatus.Inactive, stored!.Status);
    }

    [Fact]
    public async Task UpdateName_WhenStudentExists_ShouldPersistName()
    {
        // Arrange
        var student = await CreateStudentAsync();
        var request = new UpdateNameRequest { FullName = "Updated Name" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/students/{student.Id}/name", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stored = await _client.GetFromJsonAsync<StudentDto>($"/api/students/{student.Id}");
        Assert.Equal("Updated Name", stored!.FullName);
    }

    [Fact]
    public async Task UpdateEmail_WhenStudentExists_ShouldPersistEmail()
    {
        // Arrange
        var student = await CreateStudentAsync();
        var request = new UpdateEmailRequest { Email = "NEW@example.com" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/students/{student.Id}/email", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stored = await _client.GetFromJsonAsync<StudentDto>($"/api/students/{student.Id}");
        Assert.Equal("new@example.com", stored!.Email);
    }

    [Fact]
    public async Task UpdateAcademicRecord_WhenStudentIsInactive_ShouldPersistRecord()
    {
        // Arrange
        var student = await CreateStudentAsync();
        await _client.PostAsync($"/api/students/{student.Id}/deactivate", null);
        var request = new UpdateAcademicRecordRequest
        {
            GradePointAverage = 6,
            EarnedCredits = 130,
            HasActiveDisciplinaryAction = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/students/{student.Id}/academic-record", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stored = await _client.GetFromJsonAsync<StudentDto>($"/api/students/{student.Id}");
        Assert.Equal(6, stored!.GradePointAverage);
        Assert.Equal(130, stored.EarnedCredits);
        Assert.Equal(StudentStatus.Inactive, stored.Status);
    }

    [Fact]
    public async Task Graduate_WhenStudentIsEligible_ShouldPersistGraduatedStatus()
    {
        // Arrange
        var student = await CreateStudentAsync();

        // Act
        var response = await _client.PostAsync($"/api/students/{student.Id}/graduate", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stored = await _client.GetFromJsonAsync<StudentDto>($"/api/students/{student.Id}");
        Assert.Equal(StudentStatus.Graduated, stored!.Status);
    }

    [Fact]
    public async Task EvaluateGraduation_WhenStudentIsInactive_ShouldReturnIneligible()
    {
        // Arrange
        var student = await CreateStudentAsync();
        await _client.PostAsync($"/api/students/{student.Id}/deactivate", null);

        // Act
        var response = await _client.GetAsync($"/api/students/{student.Id}/graduation-eligibility");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = await System.Text.Json.JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.False(document.RootElement.GetProperty("isEligible").GetBoolean());
        Assert.NotEmpty(document.RootElement.GetProperty("reasons").EnumerateArray());
    }

    [Fact]
    public async Task GetOpenApi_WhenDocumentIsRequested_ShouldDescribeStudentRoutes()
    {
        // Act
        var response = await _client.GetAsync("/openapi/v1.json");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = await response.Content.ReadAsStringAsync();
        Assert.Contains("/api/students", document);
    }

    private static CreateStudentRequest CreateRequest(string code = "STU123456")
    {
        return new CreateStudentRequest
        {
            StudentCode = code,
            FullName = "Name",
            Email = "a@example.com",
            GradePointAverage = 5,
            EarnedCredits = 120,
            HasActiveDisciplinaryAction = false
        };
    }

    private async Task<StudentDto> CreateStudentAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/students", CreateRequest());
        response.EnsureSuccessStatusCode();
        var student = await response.Content.ReadFromJsonAsync<StudentDto>();
        return student!;
    }
}
