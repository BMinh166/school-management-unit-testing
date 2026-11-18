using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SchoolManagement.Infrastructure.Persistence;

public sealed class SchoolDbContextFactory : IDesignTimeDbContextFactory<SchoolDbContext>
{
    public SchoolDbContext CreateDbContext(string[] args)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var apiDirectory = Path.Combine(currentDirectory, "SchoolManagement.API");
        if (!Directory.Exists(apiDirectory))
        {
            var siblingApiDirectory = Path.GetFullPath(Path.Combine(currentDirectory, "..", "SchoolManagement.API"));
            if (Directory.Exists(siblingApiDirectory))
            {
                apiDirectory = siblingApiDirectory;
            }
            else
            {
                apiDirectory = currentDirectory;
            }
        }

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Development";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        var connectionString = configuration.GetConnectionString("SchoolDatabase");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:SchoolDatabase must be configured before running EF Core tools.");
        }

        var options = new DbContextOptionsBuilder<SchoolDbContext>();
        options.UseNpgsql(connectionString);
        return new SchoolDbContext(options.Options);
    }
}
