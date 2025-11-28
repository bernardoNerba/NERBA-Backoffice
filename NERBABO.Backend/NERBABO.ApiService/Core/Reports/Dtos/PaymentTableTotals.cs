


namespace NERBABO.ApiService.Core.Reports.Dtos;
public class PaymentTableTotals
{
    public Dictionary<string, float> CategoryTotals { get; }
    public float TotalHours { get; private set; }
    public int TotalDays { get; private set; }
    public float TotalPayment { get; private set; }

    public PaymentTableTotals(List<string> categories)
    {
        CategoryTotals = categories.ToDictionary(c => c, c => 0f);
    }

    public void Add(EnrollmentStats stats)
    {
        foreach (var kvp in stats.HoursByCategory)
        {
            CategoryTotals[kvp.Key] += kvp.Value;
        }
        TotalHours += stats.TotalHours;
        TotalDays += stats.TotalDays;
        TotalPayment += stats.TotalPayment;
    }
}