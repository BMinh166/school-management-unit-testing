using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.Students.DTOs;
using SchoolManagement.Application.Students.Services;
using SchoolManagement.Domain.Students.Services;

namespace SchoolManagement.API.Students;

[ApiController]
[Route("api/students")]
public sealed class StudentsController : ControllerBase
{
    private readonly StudentApplicationService _service;

    public StudentsController(StudentApplicationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<StudentDto>> Create(CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var student = await _service.CreateStudentAsync(request.StudentCode, request.FullName, request.Email,
            request.GradePointAverage!.Value, request.EarnedCredits!.Value,
            request.HasActiveDisciplinaryAction!.Value, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var student = await _service.GetStudentByIdAsync(id, cancellationToken);
        return Ok(student);
    }

    [HttpPut("{id:guid}/name")]
    public async Task<IActionResult> UpdateName(Guid id, UpdateNameRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateStudentNameAsync(id, request.FullName, cancellationToken);
        return Ok();
    }

    [HttpPut("{id:guid}/email")]
    public async Task<IActionResult> UpdateEmail(Guid id, UpdateEmailRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateStudentEmailAsync(id, request.Email, cancellationToken);
        return Ok();
    }

    [HttpPut("{id:guid}/academic-record")]
    public async Task<IActionResult> UpdateAcademicRecord(Guid id, UpdateAcademicRecordRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateAcademicRecordAsync(id, request.GradePointAverage!.Value,
            request.EarnedCredits!.Value, request.HasActiveDisciplinaryAction!.Value, cancellationToken);
        return Ok();
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _service.ActivateStudentAsync(id, cancellationToken);
        return Ok();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeactivateStudentAsync(id, cancellationToken);
        return Ok();
    }

    [HttpGet("{id:guid}/graduation-eligibility")]
    public async Task<ActionResult<GraduationEligibility>> EvaluateGraduation(Guid id, CancellationToken cancellationToken)
    {
        var eligibility = await _service.EvaluateGraduationAsync(id, cancellationToken);
        return Ok(eligibility);
    }

    [HttpPost("{id:guid}/graduate")]
    public async Task<IActionResult> Graduate(Guid id, CancellationToken cancellationToken)
    {
        await _service.GraduateStudentAsync(id, cancellationToken);
        return Ok();
    }
}
