
using NERBABO.ApiService.Core.Kpis.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Shared.Models;
using System.Globalization;
using Humanizer;

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
        string refersTo; DateTime startDate;

        (startDate, refersTo) = DefineTimeIntervaleAsync(t);

        var totalPayments = await _context.ActionEnrollments
            .AsNoTracking()
            .Where(ae => ae.PaymentDate.HasValue
                && ae.PaymentDate.Value >= DateOnly.FromDateTime(startDate))
            .SumAsync(ae => ae.PaymentTotal);

        var data = new Kpi<int>
        {
            KpiTitle = title,
            RefersTo = refersTo,
            Value = (int)Math.Ceiling(totalPayments),
        };

        return Result<Kpi<int>>
            .Ok(data);
    }

    public async Task<Result<Kpi<int>>> TeacherPayments(TimeIntervalEnum t)
    {
        string title = "Pagamentos a Formadores";
        string refersTo; DateTime startDate;

        (startDate, refersTo) = DefineTimeIntervaleAsync(t);

        var totalPayments = await _context.ModuleTeachings
            .AsNoTracking()
            .Where(ae => ae.PaymentDate.HasValue
                && ae.PaymentDate.Value >= DateOnly.FromDateTime(startDate))
            .SumAsync(ae => ae.PaymentTotal);

        var data = new Kpi<int>
        {
            KpiTitle = title,
            RefersTo = refersTo,
            Value = (int)Math.Ceiling(totalPayments)
        };

        return Result<Kpi<int>>
            .Ok(data);
    }

    public async Task<Result<Kpi<int>>> TotalCompanies(TimeIntervalEnum t)
    {
        string title = "Quantidade de Empresas";
        string refersTo; DateTime startDate;

        (startDate, refersTo) = DefineTimeIntervaleAsync(t);

        var totalCompanies = await _context.Companies
            .CountAsync(c => c.CreatedAt >= startDate);

        var data = new Kpi<int>
        {
            KpiTitle = title,
            RefersTo = refersTo,
            Value = totalCompanies
        };

        return Result<Kpi<int>>
            .Ok(data);
    }

    public async Task<Result<Kpi<List<ChartDataPoint>>>> StudentsByHabilitationLvl(TimeIntervalEnum t)
    {
        string title = "Formandos por Nível de Habilitação";
        string refersTo; DateTime startDate;

        (startDate, refersTo) = DefineTimeIntervaleAsync(t);

        var studentsByHabilitation = await _context.People
            .AsNoTracking()
            .Where(p => p.Student != null && p.CreatedAt >= startDate)
            .GroupBy(p => p.Habilitation)
            .Select(g => new
            {
                Habilitation = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        // Get all habilitation enum values with their descriptions using Humanizer
        var allHabilitations = Enum.GetValues<HabilitationEnum>()
            .Select(h => new ChartDataPoint
            {
                Label = h.Humanize().Transform(To.TitleCase),
                Value = 0
            })
            .ToList();

        // Merge with actual data
        foreach (var item in studentsByHabilitation)
        {
            var label = item.Habilitation.Humanize().Transform(To.TitleCase);
            var existing = allHabilitations.FirstOrDefault(h => h.Label == label);

            if (existing != null)
            {
                existing.Value = item.Count;
            }
        }

        var data = new Kpi<List<ChartDataPoint>>
        {
            KpiTitle = title,
            RefersTo = refersTo,
            Value = allHabilitations
        };

        return Result<Kpi<List<ChartDataPoint>>>
            .Ok(data);
    }

    public async Task<Result<Kpi<List<ChartDataPoint>>>> StudentResults(TimeIntervalEnum t)
    {
        string title = "Resultados de Formandos";
        string refersTo; DateTime startDate;

        (startDate, refersTo) = DefineTimeIntervaleAsync(t);

        var studentResults = await _context.ActionEnrollments
            .AsNoTracking()
            .Where(ae => ae.CreatedAt >= startDate)
            .Select(ae => new
            {
                ApprovalStatus = ae.ApprovalStatus
            })
            .ToListAsync();

        // Group by approval status in memory (since ApprovalStatus is a calculated property)
        var groupedResults = studentResults
            .GroupBy(ae => ae.ApprovalStatus)
            .Select(g => new
            {
                ApprovalStatus = g.Key,
                Count = g.Count()
            })
            .ToList();

        // Get all approval status enum values with their descriptions using Humanizer
        var allApprovalStatuses = Enum.GetValues<ApprovalStatusEnum>()
            .Select(a => new ChartDataPoint
            {
                Label = a.Humanize().Transform(To.TitleCase),
                Value = 0
            })
            .ToList();

        // Merge with actual data
        foreach (var item in groupedResults)
        {
            var label = item.ApprovalStatus.Humanize().Transform(To.TitleCase);
            var existing = allApprovalStatuses.FirstOrDefault(h => h.Label == label);

            if (existing != null)
            {
                existing.Value = item.Count;
            }
        }

        var data = new Kpi<List<ChartDataPoint>>
        {
            KpiTitle = title,
            RefersTo = refersTo,
            Value = allApprovalStatuses
        };

        return Result<Kpi<List<ChartDataPoint>>>
            .Ok(data);
    }

    private (DateTime d, string rf) DefineTimeIntervaleAsync(TimeIntervalEnum t)
    {
        DateTime startDate;
        string refersTo = string.Empty;
        DateTime now = DateTime.UtcNow;

        switch (t)
        {
            case TimeIntervalEnum.Month:
                startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                refersTo = now.ToString("MMMM", new CultureInfo("pt-PT"));
                break;
            case TimeIntervalEnum.Year:
                startDate = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                refersTo = now.Year.ToString();
                break;
            case TimeIntervalEnum.Ever:
                startDate = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
                refersTo = "sempre";
                break;
            default:
                startDate = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
                _logger.LogWarning("Invalid TimeIntervalEnum option.");
                break;
        }

        return (startDate, refersTo);
    }
}