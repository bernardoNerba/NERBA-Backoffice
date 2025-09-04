namespace NERBABO.ApiService.Core.Enrollments.Dtos;

public class RetrieveMTEnrollmentDto
{
    public long EnrollmentId { get; set; }
    public double Evaluation { get; set; }
    public string StudentFullName { get; set; } = string.Empty;
    public bool Approved { get; set; }
    public long ActionId { get; set; }
    public DateTime CreatedAt { get; set; }
}