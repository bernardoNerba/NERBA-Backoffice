namespace NERBABO.ApiService.Core.Enrollments.Dtos;

public class ProcessStudentsPaymentDto
{
    public long ActionEnrollmentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public double PaymentTotal { get; set; }
    public double CalculatedTotal { get; set; }
    public string PaymentDate { get; set; } = string.Empty;
    public bool IsPayed { get; set; }
}