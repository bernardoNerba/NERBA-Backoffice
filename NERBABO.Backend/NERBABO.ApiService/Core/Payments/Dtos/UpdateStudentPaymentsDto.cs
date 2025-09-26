namespace NERBABO.ApiService.Core.Payments.Dtos;

public class UpdateStudentPaymentsDto
{
    public long ActionEnrollmentId { get; set; }
    public double PaymentTotal { get; set; }
    public string PaymentDate { get; set; } = string.Empty;
}