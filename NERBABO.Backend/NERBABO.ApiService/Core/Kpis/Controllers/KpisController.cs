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

    [HttpGet("student-payments-kpi/{intervale}")]
    [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
    public async Task<IActionResult> GetStudentPaymentsKpiAsync(TimeIntervalEnum intervale)
    {
        var result = await _kpisService.StudentPayments(intervale);
        return _responseHandler.HandleResult(result);
    }

    [HttpGet("teacher-payments-kpi/{intervale}")]
    [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
    public async Task<IActionResult> GetTeacherPaymentsKpiAsync(TimeIntervalEnum intervale)
    {
        var result = await _kpisService.TeacherPayments(intervale);
        return _responseHandler.HandleResult(result);
    }

    [HttpGet("total-companies/{intervale}")]
    [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
    public async Task<IActionResult> GetTotalCompaniesKpiAsync(TimeIntervalEnum intervale)
    {
        var result = await _kpisService.TotalCompanies(intervale);
        return _responseHandler.HandleResult(result);
    }

    [HttpGet("students-by-habilitation/{intervale}")]
    [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
    public async Task<IActionResult> GetStudentsByHabilitationAsync(TimeIntervalEnum intervale)
    {
        var result = await _kpisService.StudentsByHabilitationLvl(intervale);
        return _responseHandler.HandleResult(result);
    }

    [HttpGet("student-results/{intervale}")]
    [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
    public async Task<IActionResult> GetStudentResultsAsync(TimeIntervalEnum intervale)
    {
        var result = await _kpisService.StudentResults(intervale);
        return _responseHandler.HandleResult(result);
    }
}
