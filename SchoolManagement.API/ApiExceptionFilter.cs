using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SchoolManagement.API;

public sealed class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        int status;
        string title;

        if (context.Exception is ArgumentException)
        {
            status = StatusCodes.Status400BadRequest;
            title = "The request is invalid.";
        }
        else if (context.Exception is KeyNotFoundException)
        {
            status = StatusCodes.Status404NotFound;
            title = "The student was not found.";
        }
        else if (context.Exception is InvalidOperationException)
        {
            status = StatusCodes.Status409Conflict;
            title = "The operation conflicts with the current state.";
        }
        else
        {
            return;
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = context.Exception.Message
        };
        context.Result = new ObjectResult(problem) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
