
using Microsoft.EntityFrameworkCore.Storage.Json;
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
    public long ModuleTeachingId { get; set; }
    public long StudentId { get; set; }
    public double TeacherEvaluation { get; set; }

    // Navigation Properties
    public required ModuleTeaching ModuleTeaching { get; set; }
    public required Student Student { get; set; }

    // Calculated Properties
    public bool Approved => TeacherEvaluation >= 3;

    public static RetrieveMTEnrollmentDto ConvertEntityToRetrieveDto(MTEnrollment mt)
    {
        return new RetrieveMTEnrollmentDto
        {
            EnrollmentId = mt.Id,
            TeacherEvaluation = mt.TeacherEvaluation,
            StudentFullName = mt.ModuleTeaching.Teacher.Person.FullName,
            Approved = mt.Approved,
            ModuleTeachingId = mt.ModuleTeachingId,
            CreatedAt = mt.CreatedAt
        };
    }

    public static MTEnrollment ConvertCreateDtoToEntity(CreateMTEnrollmentDto mte, ModuleTeaching mt, Student s)
    {
        return new MTEnrollment
        {
            ModuleTeachingId = mte.ModuleTeachingId,
            StudentId = mte.StudentId,
            ModuleTeaching = mt,
            Student = s
        };
    }
}