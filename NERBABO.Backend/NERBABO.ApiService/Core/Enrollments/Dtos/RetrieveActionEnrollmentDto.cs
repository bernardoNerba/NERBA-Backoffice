namespace NERBABO.ApiService.Core.Enrollments.Dtos;

public class RetrieveActionEnrollmentDto
{
    public long EnrollmentId { get; set; }
    public string StudentFullName { get; set; } = string.Empty;
    public string ApprovalStatus { get; set; } = string.Empty;
    public double AvgEvaluation { get; set; }
    public long ActionId { get; set; }
    public bool StudentAvaliated { get; set; }
    public long PersonId { get; set; }
    public long StudentId { get; set; }
    public DateTime CreatedAt { get; set; }
}