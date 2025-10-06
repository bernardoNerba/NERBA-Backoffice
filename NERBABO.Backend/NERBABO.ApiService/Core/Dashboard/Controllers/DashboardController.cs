using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Kpis.Services;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Dashboard.Controllers;

/// <summary>
/// Controller for dashboard operations.
/// Aggregates all KPI data needed for the frontend dashboard component.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class DashboardController(
    IResponseHandler responseHandler,
    IKpisService kpisService
) : ControllerBase
{
    private readonly IResponseHandler _responseHandler = responseHandler;
    private readonly IKpisService _kpisService = kpisService;

    /// <summary>
    /// Gets all dashboard data with predefined time intervals for each KPI.
    /// Aggregates multiple KPIs including payments, companies, students demographics, and action statistics.
    /// </summary>
    /// <returns>Aggregated dashboard data including KPIs, charts, and top 5 lists.</returns>
    /// <response code="200">Returns aggregated dashboard data.</response>
    /// <response code="401">Unauthorized access. Invalid JWT or user is not active.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet]
    [Authorize(Roles = "Admin, User", Policy = "ActiveUser")]
    public async Task<IActionResult> GetDashboardDataAsync()
    {
        // Fetch all KPI data sequentially to avoid DbContext concurrency issues
        // Each KPI uses its specific time interval as defined in the dashboard requirements

        // KPI Cards: Month for payments, Year for companies
        var studentPayments = await _kpisService.StudentPayments(TimeIntervalEnum.Month);
        if (!studentPayments.Success) return _responseHandler.HandleResult(studentPayments);

        var teacherPayments = await _kpisService.TeacherPayments(TimeIntervalEnum.Month);
        if (!teacherPayments.Success) return _responseHandler.HandleResult(teacherPayments);

        var totalCompanies = await _kpisService.TotalCompanies(TimeIntervalEnum.Year);
        if (!totalCompanies.Success) return _responseHandler.HandleResult(totalCompanies);

        // Charts: Year for student-related, Ever for actions
        var studentsByHabilitationLvl = await _kpisService.StudentsByHabilitationLvl(TimeIntervalEnum.Year);
        if (!studentsByHabilitationLvl.Success) return _responseHandler.HandleResult(studentsByHabilitationLvl);

        var studentResults = await _kpisService.StudentResults(TimeIntervalEnum.Year);
        if (!studentResults.Success) return _responseHandler.HandleResult(studentResults);

        var actionHabilitationsLvl = await _kpisService.ActionHabilitationsLvl(TimeIntervalEnum.Ever);
        if (!actionHabilitationsLvl.Success) return _responseHandler.HandleResult(actionHabilitationsLvl);

        var studentGenders = await _kpisService.StudentGenders(TimeIntervalEnum.Year);
        if (!studentGenders.Success) return _responseHandler.HandleResult(studentGenders);

        // Top 5 lists: Year for consistency
        var top5ActionsByLocality = await _kpisService.Top5ActionsByLocality(TimeIntervalEnum.Year);
        if (!top5ActionsByLocality.Success) return _responseHandler.HandleResult(top5ActionsByLocality);

        var top5ActionsByRegiment = await _kpisService.Top5ActionsByRegiment(TimeIntervalEnum.Year);
        if (!top5ActionsByRegiment.Success) return _responseHandler.HandleResult(top5ActionsByRegiment);

        var top5ActionsByStatus = await _kpisService.Top5ActionsByStatus(TimeIntervalEnum.Year);
        if (!top5ActionsByStatus.Success) return _responseHandler.HandleResult(top5ActionsByStatus);

        // Aggregate all data into a single response
        var dashboardData = new
        {
            // KPI cards for simple metrics
            Kpis = new
            {
                StudentPayments = studentPayments.Data,
                TeacherPayments = teacherPayments.Data,
                TotalCompanies = totalCompanies.Data
            },

            // Chart data
            Charts = new
            {
                StudentsByHabilitationLvl = studentsByHabilitationLvl.Data,
                StudentResults = studentResults.Data,
                ActionHabilitationsLvl = actionHabilitationsLvl.Data,
                StudentGenders = studentGenders.Data
            },

            // Top 5 lists for tabs
            Top5 = new
            {
                ActionsByLocality = top5ActionsByLocality.Data,
                ActionsByRegiment = top5ActionsByRegiment.Data,
                ActionsByStatus = top5ActionsByStatus.Data
            }
        };

        return Ok(dashboardData);
    }
}
