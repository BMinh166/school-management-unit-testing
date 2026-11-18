using SchoolManagement.Domain.Students.Entities;
using SchoolManagement.Domain.Students.ValueObjects;
using SchoolManagement.Domain.Students.Services;
using Xunit;

namespace SchoolManagement.Tests.Domain;

public class AcademicRecordTests
{
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

    [Fact]
    public void Constructor_WhenGpaAndCreditsAreZero_ShouldRetainValues()
    {
        // Act
        var record = new AcademicRecord(0, 0, false);

        // Assert
        Assert.Equal(0, record.GradePointAverage);
        Assert.Equal(0, record.EarnedCredits);
    }

    [Fact]
    public void Equals_WhenAnyFieldDiffers_ShouldReturnFalse()
    {
        // Arrange
        var record = new AcademicRecord(5, 120, false);
        var differentGpa = new AcademicRecord(6, 120, false);
        var differentCredits = new AcademicRecord(5, 121, false);
        var differentDiscipline = new AcademicRecord(5, 120, true);

        // Assert
        Assert.NotEqual(record, differentGpa);
        Assert.NotEqual(record, differentCredits);
        Assert.NotEqual(record, differentDiscipline);
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
