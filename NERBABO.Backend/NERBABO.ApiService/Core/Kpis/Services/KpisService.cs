
using NERBABO.ApiService.Core.Kpis.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Shared.Models;
using System.Globalization;

namespace NERBABO.ApiService.Core.Kpis.Services;

public class KpisService(
    AppDbContext context,
    ILogger<KpisService> logger
) : IKpisService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<KpisService> _logger = logger;

    public async Task<Result<Kpi<int>>> StudentPayments(TimeIntervalEnum t)
    {
        string title = "Pagamentos a Formandos";
        string refersTo = "";
        DateTime now = DateTime.UtcNow;
        DateTime startDate;

        switch (t)
        {
            case TimeIntervalEnum.Month:
                startDate = new DateTime(now.Year, now.Month, 1);
                refersTo = now.ToString("MMMM", new CultureInfo("pt-PT"));
                break;
            case TimeIntervalEnum.Year:
                startDate = new DateTime(now.Year, 1, 1);
                refersTo = now.Year.ToString();
                break;
            case TimeIntervalEnum.Ever:
                refersTo = "sempre";
                startDate = DateTime.MinValue;
                break;
            default:
                startDate = DateTime.MinValue;
                break;
        }

        var totalPayments = await _context.ActionEnrollments
            .AsNoTracking()
            .Where(ae => ae.PaymentDate.HasValue
                && ae.PaymentDate.Value >= DateOnly.FromDateTime(startDate))
            .SumAsync(ae => ae.PaymentTotal);

        return Result<Kpi<int>>.Ok(new Kpi<int>
        {
            KpiTitle = title,
            RefersTo = refersTo,
            Value = (int)Math.Ceiling(totalPayments),
            QueriedAt = now
        });
    }

    public Task<Result<Kpi<int>>> TeacherPayments(TimeIntervalEnum t)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Kpi<int>>> TotalCompanies(TimeIntervalEnum t)
    {
        throw new NotImplementedException();
    }
}