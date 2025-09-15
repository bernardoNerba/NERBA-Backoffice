namespace NERBABO.ApiService.Core.ModuleTeachings.Dtos;

public class ProcessModuleTeachingPaymentDto
{
    public long ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public double PaymentTotal { get; set; }
    public double CalculatedTotal { get; set; }
    public string PaymentDate { get; set; } = string.Empty;
    public bool IsPayed { get; set; }
}