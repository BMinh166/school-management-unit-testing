using SchoolManagement.Domain.Students.Enums;

namespace SchoolManagement.Application.Students.DTOs;

public sealed record StudentDto(Guid Id, string StudentCode, string FullName, string Email,
    StudentStatus Status, decimal GradePointAverage, int EarnedCredits, bool HasActiveDisciplinaryAction);
