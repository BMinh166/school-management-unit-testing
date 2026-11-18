using SchoolManagement.Domain.Students.Entities;
using SchoolManagement.Domain.Students.ValueObjects;
using SchoolManagement.Domain.Students.Services;
using Xunit;

namespace SchoolManagement.Tests.Domain;

public class AcademicRecordTests
{
    // TODO: Test accept GPA boundaries 0 and 10 and zero credits.

    // TODO: Test reject GPA outside 0 to 10 and negative credits.

    // TODO: Test compare all fields by value.

    [Fact]
    public void Constructor_WhenValuesAreEqual_ShouldCompareByValue()
    {
        // Arrange
        var first = new AcademicRecord(0, 0, false);
        var second = new AcademicRecord(0, 0, false);

        // Assert
        Assert.Equal(first, second);
    }

    [Fact]
    public void Constructor_WhenGpaIsTen_ShouldRetainGpa()
    {
        // Act
        var record = new AcademicRecord(10, 1, true);

        // Assert
        Assert.Equal(10, record.GradePointAverage);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void Constructor_WhenGpaIsOutsideRange_ShouldThrowArgumentOutOfRangeException(decimal gpa)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new AcademicRecord(gpa, 0, false));
    }

    [Fact]
    public void Constructor_WhenCreditsAreNegative_ShouldThrowArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new AcademicRecord(0, -1, false));
    }
}
