using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.API;
using SchoolManagement.Application.Students.Services;
using SchoolManagement.Domain.Students.Repositories;
using SchoolManagement.Domain.Students.Services;
using SchoolManagement.Infrastructure.Persistence;

var app = ApiHost.Build(args);
await app.RunAsync();

public static class ApiHost
{
    public static WebApplication Build(string[] args, Action<IServiceCollection>? configureServices = null)
    {
        var options = new WebApplicationOptions
        {
            Args = args,
            ApplicationName = typeof(ApiHost).Assembly.FullName
        };
        var builder = WebApplication.CreateBuilder(options);
        builder.Services.AddControllers(options => options.Filters.Add<ApiExceptionFilter>());
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problem = new ValidationProblemDetails(context.ModelState)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "The request is invalid."
                };
                return new BadRequestObjectResult(problem);
            };
        });
        builder.Services.AddProblemDetails();
        builder.Services.AddOpenApi();
        builder.Services.AddDbContext<SchoolDbContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("SchoolDatabase");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:SchoolDatabase must be configured.");
            }
            options.UseNpgsql(connectionString);
        });
        builder.Services.AddScoped<IStudentRepository, PostgresStudentRepository>();
        builder.Services.AddSingleton<StudentGraduationService>();
        builder.Services.AddScoped<StudentApplicationService>();
        configureServices?.Invoke(builder.Services);

        var app = builder.Build();
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.MapOpenApi();
        app.MapControllers();
        return app;
    }
}
