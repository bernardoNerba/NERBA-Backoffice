namespace NERBABO.ApiService.Core.Kpis.Models;

public class Kpi<T>
{
    public string KpiTitle { get; set; } = string.Empty;
    public DateTime QueriedAt { get; set; } = DateTime.UtcNow;
    public string RefersTo { get; set; } = string.Empty;
    public T? Value { get; set; }
}