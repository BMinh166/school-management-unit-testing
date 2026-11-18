using SchoolManagement.Domain.Students.Entities;
using SchoolManagement.Domain.Students.ValueObjects;
using SchoolManagement.Domain.Students.Services;
using Xunit;

namespace SchoolManagement.Tests.Domain;

public class StudentCodeTests
{
    [Theory]
    [InlineData("STU000001")]
    [InlineData(" STU123456 ")]
    public void Constructor_WhenValueIsValid_ShouldTrimValue(string value)
    {
        // Act
        var studentCode = new StudentCode(value);

        // Assert
        Assert.Equal(value.Trim(), studentCode.Value);
    }

    [Fact]
    public void Equals_WhenCodesDifferOnlyByWhitespace_ShouldReturnTrue()
    {
        // Arrange
        var first = new StudentCode("STU123456");
        var second = new StudentCode(" STU123456 ");

        // Assert
        Assert.Equal(first, second);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("stu000001")]
    [InlineData("STU12345")]
    [InlineData("STU12345x")]
    [InlineData("STU１２３４５６")]
    public void Constructor_WhenValueIsInvalid_ShouldThrowArgumentException(string? value)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new StudentCode(value!));
    }
}
