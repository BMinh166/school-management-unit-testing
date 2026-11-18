using Microsoft.EntityFrameworkCore;
using Npgsql;
using SchoolManagement.Domain.Students.Entities;
using SchoolManagement.Domain.Students.Repositories;
using SchoolManagement.Domain.Students.ValueObjects;

namespace SchoolManagement.Infrastructure.Persistence;

public sealed class PostgresStudentRepository : IStudentRepository
{
    private readonly SchoolDbContext _context;

    public PostgresStudentRepository(SchoolDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _context.Students.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return row?.ToStudent();
    }

    public async Task<Student?> GetByCodeAsync(StudentCode code, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(code);
        var row = await _context.Students.AsNoTracking().SingleOrDefaultAsync(item => item.StudentCode == code.Value, cancellationToken);
        return row?.ToStudent();
    }

    public async Task<bool> ExistsByCodeAsync(StudentCode code, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(code);
        return await _context.Students.AsNoTracking().AnyAsync(item => item.StudentCode == code.Value, cancellationToken);
    }

    public async Task AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(student);
        _context.Students.Add(StudentRow.FromStudent(student));
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException postgres && postgres.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new InvalidOperationException("Student ID or code already exists.", exception);
        }
    }

    public async Task UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(student);
        var row = await _context.Students.SingleOrDefaultAsync(item => item.Id == student.Id, cancellationToken);
        if (row == null)
        {
            throw new KeyNotFoundException($"Student '{student.Id}' was not found.");
        }
        if (row.StudentCode != student.StudentCode.Value)
        {
            throw new InvalidOperationException("Student code cannot change.");
        }
        row.FullName = student.FullName;
        row.Email = student.Email.Value;
        row.Status = student.Status;
        row.GradePointAverage = student.AcademicRecord.GradePointAverage;
        row.EarnedCredits = student.AcademicRecord.EarnedCredits;
        row.HasActiveDisciplinaryAction = student.AcademicRecord.HasActiveDisciplinaryAction;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
