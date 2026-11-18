using SchoolManagement.Application.Students.DTOs;
using SchoolManagement.Domain.Students.Entities;
using SchoolManagement.Domain.Students.Repositories;
using SchoolManagement.Domain.Students.Services;
using SchoolManagement.Domain.Students.ValueObjects;

namespace SchoolManagement.Application.Students.Services;

public sealed class StudentApplicationService
{
    private readonly IStudentRepository _repository;
    private readonly StudentGraduationService _graduationService;

    public StudentApplicationService(IStudentRepository repository, StudentGraduationService graduationService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _graduationService = graduationService ?? throw new ArgumentNullException(nameof(graduationService));
    }

    public async Task<StudentDto> CreateStudentAsync(string code, string fullName, string email,
        decimal gradePointAverage, int earnedCredits, bool hasActiveDisciplinaryAction, CancellationToken cancellationToken = default)
    {
        var studentCode = new StudentCode(code);
        var studentEmail = new Email(email);
        var academicRecord = new AcademicRecord(gradePointAverage, earnedCredits, hasActiveDisciplinaryAction);
        var student = new Student(studentCode, fullName, studentEmail, academicRecord);
        if (await _repository.ExistsByCodeAsync(student.StudentCode, cancellationToken))
        {
            throw new InvalidOperationException("Student code already exists.");
        }
        await _repository.AddAsync(student, cancellationToken);
        return Map(student);
    }

    public async Task<StudentDto> GetStudentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await FindAsync(id, cancellationToken);
        return Map(student);
    }

    public async Task UpdateStudentNameAsync(Guid id, string fullName, CancellationToken cancellationToken = default)
    {
        var student = await FindAsync(id, cancellationToken);
        student.UpdateFullName(fullName);
        await _repository.UpdateAsync(student, cancellationToken);
    }

    public async Task UpdateStudentEmailAsync(Guid id, string email, CancellationToken cancellationToken = default)
    {
        var student = await FindAsync(id, cancellationToken);
        student.UpdateEmail(new Email(email));
        await _repository.UpdateAsync(student, cancellationToken);
    }

    public async Task ActivateStudentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await FindAsync(id, cancellationToken);
        student.Activate();
        await _repository.UpdateAsync(student, cancellationToken);
    }

    public async Task DeactivateStudentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await FindAsync(id, cancellationToken);
        student.Deactivate();
        await _repository.UpdateAsync(student, cancellationToken);
    }

    public async Task UpdateAcademicRecordAsync(Guid id, decimal gradePointAverage, int earnedCredits,
        bool hasActiveDisciplinaryAction, CancellationToken cancellationToken = default)
    {
        var student = await FindAsync(id, cancellationToken);
        student.UpdateAcademicRecord(new AcademicRecord(gradePointAverage, earnedCredits, hasActiveDisciplinaryAction));
        await _repository.UpdateAsync(student, cancellationToken);
    }

    public async Task<GraduationEligibility> EvaluateGraduationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await FindAsync(id, cancellationToken);
        return _graduationService.Evaluate(student);
    }

    public async Task GraduateStudentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await FindAsync(id, cancellationToken);
        var eligibility = _graduationService.Evaluate(student);
        if (!eligibility.IsEligible)
        {
            throw new InvalidOperationException(string.Join(" ", eligibility.Reasons));
        }
        student.Graduate();
        await _repository.UpdateAsync(student, cancellationToken);
    }

    private async Task<Student> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Student '{id}' was not found.");
    }

    private static StudentDto Map(Student student)
    {
        return new StudentDto(
            student.Id,
            student.StudentCode.Value,
            student.FullName,
            student.Email.Value,
            student.Status,
            student.AcademicRecord.GradePointAverage,
            student.AcademicRecord.EarnedCredits,
            student.AcademicRecord.HasActiveDisciplinaryAction);
    }
}
