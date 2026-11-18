using SchoolManagement.Domain.Students.Entities;
using SchoolManagement.Domain.Students.ValueObjects;
using SchoolManagement.Domain.Students.Services;
using Xunit;

namespace SchoolManagement.Tests.Domain;

public class EmailTests
{
    [Fact]
    public void Constructor_WhenAddressHasWhitespaceAndUppercase_ShouldNormalizeAndCompareByValue()
    {
        // Arrange
        var expected = new Email("a@example.com");
        var actual = new Email(" A@EXAMPLE.COM ");

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData(" ")]
    public void Constructor_WhenAddressIsInvalid_ShouldThrowArgumentException(string value)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Email(value));
    }
}
