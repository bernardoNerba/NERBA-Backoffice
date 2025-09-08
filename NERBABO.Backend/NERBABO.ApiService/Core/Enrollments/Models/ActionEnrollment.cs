using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Enrollments.Dtos;
using NERBABO.ApiService.Core.Students.Models;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Enums;
using Humanizer;
using NERBABO.ApiService.Core.SessionParticipations.Models;
using NERBABO.ApiService.Core.ModuleAvaliations.Models;

namespace NERBABO.ApiService.Core.Enrollments.Models;

/// <summary>
/// Serves as relation N - N between Action and Student
/// </summary>
public class ActionEnrollment : Entity<long>
{
    // Instance Properties
    public long ActionId { get; set; }
    public long StudentId { get; set; }

    // Navigation Properties
    public required CourseAction Action { get; set; }
    public required Student Student { get; set; }
    public List<SessionParticipation> Participants { get; set; } = [];
    public List<ModuleAvaliation> Avaliations = [];

    // Calculated Properties
    public double AvgEvaluation =>
        Avaliations.Count != 0
        ? Avaliations.Sum(a => a.Grade) / Avaliations.Count 
        : 0;
    public bool StudentAvaliated => Avaliations.All(a => a.Grade > 0);
    public int NumberOfParticipants => Participants.Count;

    public ApprovalStatusEnum ApprovalStatus =>
        StudentAvaliated && AvgEvaluation != 0
        ? AvgEvaluation >= 3 ? ApprovalStatusEnum.Approved : ApprovalStatusEnum.Rejected
        : ApprovalStatusEnum.NotSpecified;

    public static RetrieveActionEnrollmentDto ConvertEntityToRetrieveDto(ActionEnrollment ae)
    {
        return new RetrieveActionEnrollmentDto
        {
            EnrollmentId = ae.Id,
            StudentFullName = ae.Student.Person.FullName,
            ApprovalStatus = ae.ApprovalStatus.Humanize().Transform(To.TitleCase),
            ActionId = ae.ActionId,
            StudentAvaliated = ae.StudentAvaliated,
            AvgEvaluation = ae.AvgEvaluation,
            PersonId = ae.Student.Person.Id,
            StudentId = ae.StudentId,
            CreatedAt = ae.CreatedAt
        };
    }

    public static ActionEnrollment ConvertCreateDtoToEntity(CreateActionEnrollmentDto ae, CourseAction a, Student s)
    {
        return new ActionEnrollment
        {
            ActionId = ae.ActionId,
            StudentId = ae.StudentId,
            Action = a,
            Student = s,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}