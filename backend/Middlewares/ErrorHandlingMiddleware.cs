using System.Net;
using System.Text.Json;
using BibliotecaAPI.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var problemDetails = exception switch
        {
            ConflictException conflictEx => new ProblemDetails
            {
                Title = "Conflito de negócio",
                Status = (int)HttpStatusCode.Conflict,
                Detail = conflictEx.Message
            },
            NotFoundException notFoundEx => new ProblemDetails
            {
                Title = "Recurso não encontrado",
                Status = (int)HttpStatusCode.NotFound,
                Detail = notFoundEx.Message
            },
            _ => new ProblemDetails
            {
                Title = "Erro interno no servidor",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = "Ocorreu um erro inesperado no servidor."
            }
        };

        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }
}