

namespace NERBABO.ApiService.Core.Reports.Dtos;

public class EnrollmentStats
{
    public Dictionary<string, float> HoursByCategory { get; }
    public float TotalHours { get; set; }
    public int TotalDays { get; set; }
    public float TotalPayment { get; set; }

    public EnrollmentStats(List<string> categories)
    {
        HoursByCategory = categories.ToDictionary(c => c, c => 0f);
    }
}