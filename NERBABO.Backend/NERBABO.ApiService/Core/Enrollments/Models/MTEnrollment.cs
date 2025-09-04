
using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Enrollments.Dtos;
using NERBABO.ApiService.Core.ModuleTeachings.Models;
using NERBABO.ApiService.Core.Students.Models;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Enrollments.Models;

/// <summary>
/// Module Teaching Student enrrolement
/// </summary>
public class MTEnrollment : Entity<long>
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

    public static RetrieveMTEnrollmentDto ConvertEntityToRetrieveDto(MTEnrollment mt)
    {
        return new RetrieveMTEnrollmentDto
        {
            EnrollmentId = mt.Id,
            Evaluation = mt.Evaluation,
            StudentFullName = mt.Student.Person.FullName,
            Approved = mt.Approved,
            ActionId = mt.ActionId,
            CreatedAt = mt.CreatedAt
        };
    }

    public static MTEnrollment ConvertCreateDtoToEntity(CreateMTEnrollmentDto mte, CourseAction a, Student s)
    {
        return new MTEnrollment
        {
            ActionId = mte.ActionId,
            StudentId = mte.StudentId,
            Action = a,
            Student = s,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}