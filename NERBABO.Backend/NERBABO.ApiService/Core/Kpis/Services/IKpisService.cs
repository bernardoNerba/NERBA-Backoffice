using NERBABO.ApiService.Core.Kpis.Models;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Kpis.Services;

public interface IKpisService
{
    /// <summary>
    /// Total teacher payments in a intervale of time.
    /// </summary>
    /// <param name="t">t is a TimeInterval that will be checked to make the query.</param>
    /// <returns>KPI where the T is a int representing the total payments to Teachers during a time period.</returns>
    Task<Result<Kpi<int>>> TeacherPayments(TimeIntervalEnum t);

    /// <summary>
    /// Total student payments in a intervale of time.
    /// </summary>
    /// <param name="t">t is a TimeInterval that will be checked to make the query.</param>
    /// <returns>KPI where the T is a int representing the total payments to Students during a time period.</returns>
    Task<Result<Kpi<int>>> StudentPayments(TimeIntervalEnum t);

    /// <summary>
    /// Total companies registed in the system during a intervale of time.
    /// </summary>
    /// <param name="t">t is a TimeInterval that will be checked to make the query.</param>
    /// <returns>KPI where the T is a int representing the total companies registed during a time period.</returns>
    Task<Result<Kpi<int>>> TotalCompanies(TimeIntervalEnum t);

    /// <summary>
    /// Distribution of students by habilitation level.
    /// </summary>
    /// <param name="t">t is a TimeInterval that will be checked to make the query.</param>
    /// <returns>KPI where the T is a list of chart data points representing the count of students per habilitation level.</returns>
    Task<Result<Kpi<List<ChartDataPoint>>>> StudentsByHabilitationLvl(TimeIntervalEnum t);

    /// <summary>
    /// Distribution of student results by approval status.
    /// </summary>
    /// <param name="t">t is a TimeInterval that will be checked to make the query.</param>
    /// <returns>KPI where the T is a list of chart data points representing the count of students per approval status.</returns>
    Task<Result<Kpi<List<ChartDataPoint>>>> StudentResults(TimeIntervalEnum t);

    /// <summary>
    /// Distribution of actions by minimum habilitation level grouped into three levels (Nível 1, 2, 3).
    /// </summary>
    /// <param name="t">t is a TimeInterval that will be checked to make the query.</param>
    /// <returns>KPI where the T is a list of chart data points representing the count of actions per habilitation level.</returns>
    Task<Result<Kpi<List<ChartDataPoint>>>> ActionHabilitationsLvl(TimeIntervalEnum t);

    /// <summary>
    /// Distribution of students by gender over time, grouped by month within the time interval.
    /// </summary>
    /// <param name="t">t is a TimeInterval that will be checked to make the query.</param>
    /// <returns>KPI where the T is a list of gender time series representing the count of students per gender per month.</returns>
    Task<Result<Kpi<List<GenderTimeSeries>>>> StudentGenders(TimeIntervalEnum t);
}