namespace NERBABO.ApiService.Core.Payments.Dtos;

public class TeacherPaymentsDto
{
    public long ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public long TeacherPersonId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public double PaymentTotal { get; set; }
    public double CalculatedTotal { get; set; }
    public string PaymentDate { get; set; } = string.Empty;
    public bool PaymentProcessed { get; set; }
}