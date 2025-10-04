using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Kpis.Services;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Kpis.Controllers;

[Route("api/[controller]")]
[ApiController]
public class KpisController(
    IResponseHandler responseHandler,
    IKpisService kpisService
) : ControllerBase
{
    private readonly IResponseHandler _responseHandler = responseHandler;
    private readonly IKpisService _kpisService = kpisService;

    /// <summary>
    /// Gets the total student payments KPI for a specified time interval.
    /// </summary>
    /// <param name="intervale">The time interval to filter the payments (Month, Year, or Ever).</param>
    /// <response code="200">Returns the student payments KPI.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet("student-payments-kpi/{intervale}")]
    [Authorize(Roles = "Admin, User", Policy = "ActiveUser")]
    public async Task<IActionResult> GetStudentPaymentsKpiAsync(TimeIntervalEnum intervale)
    {
        var result = await _kpisService.StudentPayments(intervale);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Gets the total teacher payments KPI for a specified time interval.
    /// </summary>
    /// <param name="intervale">The time interval to filter the payments (Month, Year, or Ever).</param>
    /// <response code="200">Returns the teacher payments KPI.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet("teacher-payments-kpi/{intervale}")]
    [Authorize(Roles = "Admin, User", Policy = "ActiveUser")]
    public async Task<IActionResult> GetTeacherPaymentsKpiAsync(TimeIntervalEnum intervale)
    {
        var result = await _kpisService.TeacherPayments(intervale);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Gets the total companies registered KPI for a specified time interval.
    /// </summary>
    /// <param name="intervale">The time interval to filter the companies (Month, Year, or Ever).</param>
    /// <response code="200">Returns the total companies KPI.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet("total-companies/{intervale}")]
    [Authorize(Roles = "Admin, User", Policy = "ActiveUser")]
    public async Task<IActionResult> GetTotalCompaniesKpiAsync(TimeIntervalEnum intervale)
    {
        var result = await _kpisService.TotalCompanies(intervale);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Gets the distribution of students by habilitation level for a specified time interval.
    /// </summary>
    /// <param name="intervale">The time interval to filter the students (Month, Year, or Ever).</param>
    /// <response code="200">Returns the students by habilitation level KPI as chart data points.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet("students-by-habilitation/{intervale}")]
    [Authorize(Roles = "Admin, User", Policy = "ActiveUser")]
    public async Task<IActionResult> GetStudentsByHabilitationAsync(TimeIntervalEnum intervale)
    {
        var result = await _kpisService.StudentsByHabilitationLvl(intervale);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Gets the distribution of student results by approval status for a specified time interval.
    /// </summary>
    /// <param name="intervale">The time interval to filter the student results (Month, Year, or Ever).</param>
    /// <response code="200">Returns the student results KPI as chart data points.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet("student-results/{intervale}")]
    [Authorize(Roles = "Admin, User", Policy = "ActiveUser")]
    public async Task<IActionResult> GetStudentResultsAsync(TimeIntervalEnum intervale)
    {
        var result = await _kpisService.StudentResults(intervale);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Gets the distribution of actions by minimum habilitation level grouped into three levels (Nível 1, 2, 3) for a specified time interval.
    /// </summary>
    /// <param name="intervale">The time interval to filter the actions (Month, Year, or Ever).</param>
    /// <response code="200">Returns the actions by habilitation level KPI as chart data points.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet("actions-by-habilitation-lvl/{intervale}")]
    [Authorize(Roles = "Admin, User", Policy = "ActiveUser")]
    public async Task<IActionResult> GetActionsByHabilitationLvlAsync(TimeIntervalEnum intervale)
    {
        var result = await _kpisService.ActionHabilitationsLvl(intervale);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Gets the distribution of students by gender over time, grouped by month within the specified time interval.
    /// </summary>
    /// <param name="intervale">The time interval to filter the students (Month, Year, or Ever).</param>
    /// <response code="200">Returns the students by gender over time KPI as gender time series data.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet("students-by-gender-over-time/{intervale}")]
    [Authorize(Roles = "Admin, User", Policy = "ActiveUser")]
    public async Task<IActionResult> GetStudentsByGenderOverTimeAsync(TimeIntervalEnum intervale)
    {
        var result = await _kpisService.StudentGenders(intervale);
        return _responseHandler.HandleResult(result);
    }
}
