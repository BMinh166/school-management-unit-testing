namespace SchoolManagement.Domain.Students.ValueObjects;

public sealed record StudentCode
{
    public string Value { get; }

    public StudentCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Student code is required.", nameof(value));
        }
        value = value.Trim();
        if (value.Length != 9 || !value.StartsWith("STU", StringComparison.Ordinal) ||
            value.AsSpan(3).ContainsAnyExceptInRange('0', '9'))
        {
            throw new ArgumentException("Student code must be STU followed by exactly six digits.", nameof(value));
        }
        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }
}
