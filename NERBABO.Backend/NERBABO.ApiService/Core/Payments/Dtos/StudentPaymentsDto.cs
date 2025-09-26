namespace NERBABO.ApiService.Core.Payments.Dtos;

public class StudentPaymentsDto
{
    public long ActionEnrollmentId { get; set; }
    public long ActionId { get; set; }
    public string ActionTitle { get; set; } = string.Empty;
    public long StudentPersonId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public double PaymentTotal { get; set; }
    public double CalculatedTotal { get; set; }
    public string PaymentDate { get; set; } = string.Empty;
    public bool PaymentProcessed { get; set; }
}