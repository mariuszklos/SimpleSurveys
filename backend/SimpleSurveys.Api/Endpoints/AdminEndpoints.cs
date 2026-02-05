using SimpleSurveys.Api.DTOs;
using SimpleSurveys.Api.Services;

namespace SimpleSurveys.Api.Endpoints;

public static class AdminEndpoints
{
    private const string AdminSessionCookie = "admin_session";
    private const string AdminSessionValue = "authenticated";

    public static void MapAdminEndpoints(this WebApplication app, IConfiguration config)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin");

        group.MapPost("/login", (AdminLoginRequest request, HttpContext context) =>
            Login(request, context, config));

        group.MapPost("/logout", Logout);
        group.MapGet("/check", CheckAuth);

        // Protected endpoints
        group.MapGet("/surveys", GetAllSurveys).AddEndpointFilter<AdminAuthFilter>();
        group.MapGet("/surveys/{id:guid}", GetSurvey).AddEndpointFilter<AdminAuthFilter>();
        group.MapGet("/surveys/{id:guid}/voters", GetSurveyVoters).AddEndpointFilter<AdminAuthFilter>();
        group.MapPost("/surveys", CreateSurvey).AddEndpointFilter<AdminAuthFilter>();
        group.MapPut("/surveys/{id:guid}", UpdateSurvey).AddEndpointFilter<AdminAuthFilter>();
        group.MapDelete("/surveys/{id:guid}", DeleteSurvey).AddEndpointFilter<AdminAuthFilter>();
    }

    private static IResult Login(AdminLoginRequest request, HttpContext context, IConfiguration config)
    {
        var storedHash = config["AdminPasswordHash"];

        if (string.IsNullOrEmpty(storedHash))
        {
            // Fallback: compare plain password if hash not configured
            var plainPassword = config["AdminPassword"];
            if (request.Password != plainPassword)
                return Results.Unauthorized();
        }
        else
        {
            if (!BCrypt.Net.BCrypt.Verify(request.Password, storedHash))
                return Results.Unauthorized();
        }

        context.Response.Cookies.Append(AdminSessionCookie, AdminSessionValue, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = true,
            MaxAge = TimeSpan.FromHours(24)
        });

        return Results.Ok(new { message = "Login successful" });
    }

    private static IResult Logout(HttpContext context)
    {
        context.Response.Cookies.Delete(AdminSessionCookie);
        return Results.Ok(new { message = "Logged out" });
    }

    private static IResult CheckAuth(HttpContext context)
    {
        var session = context.Request.Cookies[AdminSessionCookie];
        var isAuthenticated = session == AdminSessionValue;
        return Results.Ok(new { authenticated = isAuthenticated });
    }

    private static async Task<IResult> GetAllSurveys(SurveyService surveyService)
    {
        var surveys = await surveyService.GetAllSurveysAsync();
        return Results.Ok(surveys);
    }

    private static async Task<IResult> GetSurvey(Guid id, SurveyService surveyService)
    {
        var survey = await surveyService.GetSurveyAsync(id, null);
        if (survey == null)
            return Results.NotFound(new { error = "Survey not found" });

        return Results.Ok(survey);
    }

    private static async Task<IResult> GetSurveyVoters(Guid id, SurveyService surveyService)
    {
        var voters = await surveyService.GetSurveyVotersAsync(id);
        if (voters == null)
            return Results.NotFound(new { error = "Survey not found" });

        return Results.Ok(voters);
    }

    private static async Task<IResult> CreateSurvey(CreateSurveyRequest request, SurveyService surveyService)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Results.BadRequest(new { error = "Title is required" });

        if (request.Options.Count < 2)
            return Results.BadRequest(new { error = "At least 2 options are required" });

        var survey = await surveyService.CreateSurveyAsync(request);
        return Results.Created($"/api/admin/surveys/{survey.Id}", new { id = survey.Id });
    }

    private static async Task<IResult> UpdateSurvey(
        Guid id,
        UpdateSurveyRequest request,
        SurveyService surveyService)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Results.BadRequest(new { error = "Title is required" });

        if (request.Options.Count < 2)
            return Results.BadRequest(new { error = "At least 2 options are required" });

        var (success, error) = await surveyService.UpdateSurveyAsync(id, request);
        if (!success)
            return Results.NotFound(new { error });

        return Results.Ok(new { message = "Survey updated" });
    }

    private static async Task<IResult> DeleteSurvey(Guid id, SurveyService surveyService)
    {
        var deleted = await surveyService.DeleteSurveyAsync(id);
        if (!deleted)
            return Results.NotFound(new { error = "Survey not found" });

        return Results.Ok(new { message = "Survey deleted" });
    }
}

public class AdminAuthFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var session = httpContext.Request.Cookies["admin_session"];

        if (session != "authenticated")
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }
}
