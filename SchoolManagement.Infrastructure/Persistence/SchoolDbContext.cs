using Microsoft.EntityFrameworkCore;

namespace SchoolManagement.Infrastructure.Persistence;

public sealed class SchoolDbContext : DbContext
{
    public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options)
    {
    }

    internal DbSet<StudentRow> Students => Set<StudentRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var student = modelBuilder.Entity<StudentRow>();
        student.ToTable("Students");
        student.HasKey(row => row.Id);
        student.Property(row => row.Id).ValueGeneratedNever();
        student.Property(row => row.StudentCode).IsRequired().HasMaxLength(9);
        student.HasIndex(row => row.StudentCode).IsUnique();
        student.Property(row => row.FullName).IsRequired();
        student.Property(row => row.Email).IsRequired();
        student.Property(row => row.Status).HasConversion<int>();
    }
}
