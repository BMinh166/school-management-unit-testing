using SchoolManagement.Domain.Students.Entities;
using SchoolManagement.Domain.Students.ValueObjects;

namespace SchoolManagement.Domain.Students.Repositories;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Student?> GetByCodeAsync(StudentCode code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(StudentCode code, CancellationToken cancellationToken = default);
    Task AddAsync(Student student, CancellationToken cancellationToken = default);
    Task UpdateAsync(Student student, CancellationToken cancellationToken = default);
}
