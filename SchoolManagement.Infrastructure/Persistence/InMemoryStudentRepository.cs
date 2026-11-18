using SchoolManagement.Domain.Students.Entities;
using SchoolManagement.Domain.Students.Repositories;
using SchoolManagement.Domain.Students.ValueObjects;

namespace SchoolManagement.Infrastructure.Persistence;

public sealed class InMemoryStudentRepository : IStudentRepository
{
    private readonly Dictionary<Guid, Student> _students = [];
    private readonly object _sync = new object();

    // Explicit opt-in keeps an empty repository convenient for unit tests.
    public InMemoryStudentRepository(bool seed = false)
    {
        if (seed)
        {
            foreach (var student in StudentSeedData.Create())
            {
                _students.Add(student.Id, student.Copy());
            }
        }
    }

    public Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            return Task.FromResult(_students.TryGetValue(id, out var student) ? student.Copy() : null);
        }
    }

    public Task<Student?> GetByCodeAsync(StudentCode code, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(code);
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            return Task.FromResult(_students.Values.FirstOrDefault(s => s.StudentCode == code)?.Copy());
        }
    }

    public Task<bool> ExistsByCodeAsync(StudentCode code, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(code);
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            return Task.FromResult(_students.Values.Any(s => s.StudentCode == code));
        }
    }

    public Task AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(student);
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            if (_students.ContainsKey(student.Id))
            {
                throw new InvalidOperationException("Student ID already exists.");
            }
            if (_students.Values.Any(s => s.StudentCode == student.StudentCode))
            {
                throw new InvalidOperationException("Student code already exists.");
            }
            _students.Add(student.Id, student.Copy());
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(student);
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            if (!_students.TryGetValue(student.Id, out var existing))
            {
                throw new KeyNotFoundException($"Student '{student.Id}' was not found.");
            }
            if (existing.StudentCode != student.StudentCode)
            {
                throw new InvalidOperationException("Student code cannot change.");
            }
            _students[student.Id] = student.Copy();
        }
        return Task.CompletedTask;
    }
}
