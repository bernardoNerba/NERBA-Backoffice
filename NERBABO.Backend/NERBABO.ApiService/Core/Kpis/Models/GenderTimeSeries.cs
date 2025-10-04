namespace NERBABO.ApiService.Core.Kpis.Models;

public class GenderTimeSeries
{
    public string Gender { get; set; } = string.Empty;
    public List<MonthDataPoint> Data { get; set; } = [];
}

public class MonthDataPoint
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}
