using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.API.Students;

public sealed class CreateStudentRequest
{
    [Required]
    public string StudentCode { get; init; } = null!;

    [Required]
    public string FullName { get; init; } = null!;

    [Required]
    public string Email { get; init; } = null!;

    [Required]
    public decimal? GradePointAverage { get; init; }

    [Required]
    public int? EarnedCredits { get; init; }

    [Required]
    public bool? HasActiveDisciplinaryAction { get; init; }
}

public sealed class UpdateNameRequest
{
    [Required]
    public string FullName { get; init; } = null!;
}

public sealed class UpdateEmailRequest
{
    [Required]
    public string Email { get; init; } = null!;
}

public sealed class UpdateAcademicRecordRequest
{
    [Required]
    public decimal? GradePointAverage { get; init; }

    [Required]
    public int? EarnedCredits { get; init; }

    [Required]
    public bool? HasActiveDisciplinaryAction { get; init; }
}
