namespace NERBABO.ApiService.Core.Payments.Dtos;

public class UpdateTeacherPaymentsDto
{
    public long ModuleTeachingId { get; set; }
    public double PaymentTotal { get; set; }
    public string PaymentDate { get; set; } = string.Empty;
}