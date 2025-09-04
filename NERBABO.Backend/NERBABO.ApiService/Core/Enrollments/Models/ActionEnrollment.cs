using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Enrollments.Dtos;
using NERBABO.ApiService.Core.ModuleTeachings.Models;
using NERBABO.ApiService.Core.Students.Models;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Enrollments.Models;

/// <summary>
/// Action Student enrollment
/// </summary>
public class ActionEnrollment : Entity<long>
{
    // Instance Properties
    public long ActionId { get; set; }
    public long StudentId { get; set; }
    public double Evaluation { get; set; }

    // Navigation Properties
    public required CourseAction Action { get; set; }
    public required Student Student { get; set; }

    // Calculated Properties
    public bool Approved => Evaluation >= 3;

    public static RetrieveActionEnrollmentDto ConvertEntityToRetrieveDto(ActionEnrollment ae)
    {
        return new RetrieveActionEnrollmentDto
        {
            EnrollmentId = ae.Id,
            Evaluation = ae.Evaluation,
            StudentFullName = ae.Student.Person.FullName,
            Approved = ae.Approved,
            ActionId = ae.ActionId,
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