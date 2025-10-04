
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

    /// <summary>
    /// Calculates the total payments made to teachers within a specified time interval.
    /// </summary>
    /// <param name="t">The time interval to filter the payments (Month, Year, or Ever).</param>
    /// <returns>A KPI containing the total amount of teacher payments as an integer.</returns>
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

    public async Task<Result<Kpi<List<ChartDataPoint>>>> ActionHabilitationsLvl(TimeIntervalEnum t)
    {
        string title = "Ações por Nível de Habilitação Mínimo";
        string refersTo; DateTime startDate;

        (startDate, refersTo) = DefineTimeIntervaleAsync(t);

        var actionsByHabilitation = await _context.Actions
            .AsNoTracking()
            .Include(a => a.Course)
            .Where(a => a.CreatedAt >= startDate)
            .Select(a => new
            {
                MinHabilitation = a.Course.MinHabilitationLevel
            })
            .ToListAsync();

        // Group by habilitation level in memory
        var groupedByLevel = actionsByHabilitation
            .GroupBy(a => MapHabilitationToLevel(a.MinHabilitation))
            .Select(g => new ChartDataPoint
            {
                Label = g.Key,
                Value = g.Count()
            })
            .ToList();

        // Ensure all levels are present, even if count is 0
        var allLevels = new List<string> { "Nível 1", "Nível 2", "Nível 3" };
        var result = allLevels.Select(level => new ChartDataPoint
        {
            Label = level,
            Value = groupedByLevel.FirstOrDefault(g => g.Label == level)?.Value ?? 0
        }).ToList();

        var data = new Kpi<List<ChartDataPoint>>
        {
            KpiTitle = title,
            RefersTo = refersTo,
            Value = result
        };

        return Result<Kpi<List<ChartDataPoint>>>
            .Ok(data);
    }

    public async Task<Result<Kpi<List<GenderTimeSeries>>>> StudentGenders(TimeIntervalEnum t)
    {
        string title = "Formandos por Género ao Longo do Tempo";
        string refersTo; DateTime startDate;

        (startDate, refersTo) = DefineTimeIntervaleAsync(t);

        var students = await _context.People
            .AsNoTracking()
            .Where(p => p.Student != null && p.CreatedAt >= startDate)
            .Select(p => new
            {
                Gender = p.Gender,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        // Get all gender enum values
        var allGenders = Enum.GetValues<GenderEnum>();

        // Generate list of months in the time interval
        // For "Ever", use a reasonable time range (last 12 months) to avoid generating thousands of months
        var effectiveStartDate = t == TimeIntervalEnum.Ever
            ? DateTime.UtcNow.AddYears(-1)
            : startDate;
        var months = GenerateMonthsList(effectiveStartDate, DateTime.UtcNow);

        // Group by gender and create time series for each
        var genderTimeSeries = allGenders.Select(gender =>
        {
            var genderLabel = gender.Humanize().Transform(To.TitleCase);
            var genderStudents = students.Where(s => s.Gender == gender);

            var monthlyData = months.Select(month =>
            {
                var count = genderStudents.Count(s =>
                    s.CreatedAt.Year == month.Year && s.CreatedAt.Month == month.Month);

                return new MonthDataPoint
                {
                    Month = month.ToString("MMM yyyy", new CultureInfo("pt-PT")),
                    Count = count
                };
            }).ToList();

            return new GenderTimeSeries
            {
                Gender = genderLabel,
                Data = monthlyData
            };
        }).ToList();

        var data = new Kpi<List<GenderTimeSeries>>
        {
            KpiTitle = title,
            RefersTo = refersTo,
            Value = genderTimeSeries
        };

        return Result<Kpi<List<GenderTimeSeries>>>
            .Ok(data);
    }

    private static string MapHabilitationToLevel(HabilitationEnum habilitation)
    {
        return habilitation switch
        {
            HabilitationEnum.WithoutProof => "Nível 1",
            HabilitationEnum.WithoutHabilitation => "Nível 1",
            HabilitationEnum.FirstYear => "Nível 1",
            HabilitationEnum.SecondYear => "Nível 1",
            HabilitationEnum.ThirdYear => "Nível 1",
            HabilitationEnum.FourthYear => "Nível 1",
            HabilitationEnum.FifthYear => "Nível 1",
            HabilitationEnum.SixthYear => "Nível 1",
            HabilitationEnum.SeventhYear => "Nível 1",
            HabilitationEnum.EighthYear => "Nível 1",
            HabilitationEnum.NinthYear => "Nível 1",
            HabilitationEnum.TenthYear => "Nível 2",
            HabilitationEnum.EleventhYear => "Nível 2",
            HabilitationEnum.TwelfthYear => "Nível 2",
            HabilitationEnum.PostSecondary => "Nível 3",
            HabilitationEnum.Bachelors => "Nível 3",
            HabilitationEnum.Undergraduate => "Nível 3",
            HabilitationEnum.Masters => "Nível 3",
            HabilitationEnum.Doctorate => "Nível 3",
            _ => "Nível 1"
        };
    }

    private static List<DateTime> GenerateMonthsList(DateTime startDate, DateTime endDate)
    {
        var months = new List<DateTime>();
        var current = new DateTime(startDate.Year, startDate.Month, 1);
        var end = new DateTime(endDate.Year, endDate.Month, 1);

        while (current <= end)
        {
            months.Add(current);
            current = current.AddMonths(1);
        }

        return months;
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