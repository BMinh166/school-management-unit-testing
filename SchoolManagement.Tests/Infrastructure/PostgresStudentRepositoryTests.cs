using Microsoft.EntityFrameworkCore;
using Npgsql;
using SchoolManagement.Domain.Students.Entities;
using SchoolManagement.Domain.Students.Enums;
using SchoolManagement.Domain.Students.ValueObjects;
using SchoolManagement.Infrastructure.Persistence;

namespace SchoolManagement.Tests.Infrastructure;

public sealed class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SCHOOL_TEST_DATABASE")))
        {
            Skip = "Set SCHOOL_TEST_DATABASE to a dedicated PostgreSQL database ending in _test.";
        }
    }
}

[Trait("Category", "PostgreSQL")]
public class PostgresStudentRepositoryTests : IAsyncLifetime
{
    private string _connectionString = null!;
    private string _schema = null!;

    public async Task InitializeAsync()
    {
        var testConnection = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("SCHOOL_TEST_DATABASE"));
        if (string.IsNullOrWhiteSpace(testConnection.Database) || !testConnection.Database.EndsWith("_test", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The integration test database name must end in _test.");
        }

        var development = Environment.GetEnvironmentVariable("ConnectionStrings__SchoolDatabase");
        if (!string.IsNullOrWhiteSpace(development))
        {
            var developmentConnection = new NpgsqlConnectionStringBuilder(development);
            if (string.Equals(testConnection.Host, developmentConnection.Host, StringComparison.OrdinalIgnoreCase)
                && testConnection.Port == developmentConnection.Port
                && string.Equals(testConnection.Database, developmentConnection.Database, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The test database must differ from the configured application database.");
            }
        }

        _schema = "test_" + Guid.NewGuid().ToString("N");
        await using var connection = new NpgsqlConnection(testConnection.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE SCHEMA \"{_schema}\"", connection);
        await command.ExecuteNonQueryAsync();
        testConnection.SearchPath = _schema;
        _connectionString = testConnection.ConnectionString;
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_connectionString == null || _schema == null)
        {
            return;
        }
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP SCHEMA IF EXISTS \"{_schema}\" CASCADE", connection);
        await command.ExecuteNonQueryAsync();
    }

    [PostgreSqlFact]
    public async Task AddAsync_WhenStudentIsValid_ShouldPersistValueObjectsAcrossContexts()
    {
        // Arrange
        var student = CreateStudent();
        await using var writeContext = CreateContext();
        var writer = new PostgresStudentRepository(writeContext);

        // Act
        await writer.AddAsync(student);

        // Assert
        await using var readContext = CreateContext();
        var reader = new PostgresStudentRepository(readContext);
        var stored = await reader.GetByIdAsync(student.Id);
        Assert.NotNull(stored);
        Assert.Equal(student.StudentCode, stored.StudentCode);
        Assert.Equal(student.Email, stored.Email);
        Assert.Equal(student.AcademicRecord, stored.AcademicRecord);
        Assert.Equal(student.FullName, stored.FullName);
    }

    [PostgreSqlFact]
    public async Task GetByCodeAsync_WhenStudentExists_ShouldReturnDetachedStudent()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new PostgresStudentRepository(context);
        var student = CreateStudent();
        await repository.AddAsync(student);

        // Act
        var copy = await repository.GetByCodeAsync(new StudentCode(" STU123456 "));
        copy!.UpdateFullName("Changed");

        // Assert
        Assert.True(await repository.ExistsByCodeAsync(student.StudentCode));
        var stored = await repository.GetByIdAsync(student.Id);
        Assert.Equal("Name", stored!.FullName);
    }

    [PostgreSqlFact]
    public async Task UpdateAsync_WhenStudentGraduates_ShouldPersistTerminalStatus()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new PostgresStudentRepository(context);
        var student = CreateStudent();
        await repository.AddAsync(student);
        student.Graduate();

        // Act
        await repository.UpdateAsync(student);

        // Assert
        await using var readContext = CreateContext();
        var reader = new PostgresStudentRepository(readContext);
        var stored = await reader.GetByIdAsync(student.Id);
        Assert.Equal(StudentStatus.Graduated, stored!.Status);
    }

    [PostgreSqlFact]
    public async Task UpdateAsync_WhenStudentIsInactive_ShouldPersistRecordAndStatus()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new PostgresStudentRepository(context);
        var student = CreateStudent();
        await repository.AddAsync(student);
        student.Deactivate();
        var academicRecord = new AcademicRecord(7.25m, 130, true);
        student.UpdateAcademicRecord(academicRecord);

        // Act
        await repository.UpdateAsync(student);

        // Assert
        await using var readContext = CreateContext();
        var reader = new PostgresStudentRepository(readContext);
        var stored = await reader.GetByIdAsync(student.Id);
        Assert.Equal(StudentStatus.Inactive, stored!.Status);
        Assert.Equal(academicRecord, stored.AcademicRecord);
    }

    [PostgreSqlFact]
    public async Task AddAsync_WhenStudentCodeAlreadyExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new PostgresStudentRepository(context);
        await repository.AddAsync(CreateStudent());
        var duplicate = CreateStudent();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.AddAsync(duplicate));
    }

    [PostgreSqlFact]
    public async Task UpdateAsync_WhenStudentDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new PostgresStudentRepository(context);
        var student = CreateStudent();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.UpdateAsync(student));
    }

    private SchoolDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SchoolDbContext>();
        options.UseNpgsql(_connectionString);
        return new SchoolDbContext(options.Options);
    }

    private static Student CreateStudent()
    {
        var code = new StudentCode("STU123456");
        var email = new Email("A@example.com");
        var record = new AcademicRecord(5, 120, false);
        return new Student(code, "Name", email, record);
    }
}
