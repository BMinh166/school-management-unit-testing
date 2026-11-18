using SchoolManagement.Domain.Students.Entities;
using SchoolManagement.Domain.Students.ValueObjects;
using SchoolManagement.Domain.Students.Services;
using Xunit;

namespace SchoolManagement.Tests.Domain;

public class StudentCodeTests
{
    // TODO: Test accept STU followed by exactly six ASCII digits.

    // TODO: Test reject missing, malformed, and incorrectly cased codes.

    // TODO: Test trim and compare codes by value.

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
    [Theory]
    [InlineData("")]
    [InlineData("stu000001")]
    [InlineData("STU12345")]
    [InlineData("STU12345x")]
    [InlineData("STU１２３４５６")]
    public void Constructor_WhenValueIsInvalid_ShouldThrowArgumentException(string value)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new StudentCode(value));
    }
}
