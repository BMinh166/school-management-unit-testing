using System.Text.RegularExpressions;

namespace SchoolManagement.Domain.Students.ValueObjects;

public sealed record Email
{
    private static readonly Regex Format =
        new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email is required.", nameof(value));
        }
        var normalized = value.Trim().ToLowerInvariant();
        if (!Format.IsMatch(normalized))
        {
            throw new ArgumentException("Email format is invalid.", nameof(value));
        }
        Value = normalized;
    }

    public override string ToString()
    {
        return Value;
    }
}
