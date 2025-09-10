using NERBABO.ApiService.Core.Enrollments.Models;
using NERBABO.ApiService.Core.ModuleAvaliations.Dtos;
using NERBABO.ApiService.Core.ModuleTeachings.Models;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.ModuleAvaliations.Models;

public class ModuleAvaliation : Entity<long>
{
    public long ModuleTeachingId { get; set; }
    public long ActionEnrollmentId { get; set; }
    public int Grade { get; set; }

    // Navigation Properties
    public required ModuleTeaching ModuleTeaching { get; set; }
    public required ActionEnrollment ActionEnrollment { get; set; }

    public static RetrieveModuleAvaliationDto ConvertEntityToEntityDto(ModuleAvaliation ma)
    {
        return new RetrieveModuleAvaliationDto
        {
            ActionId = ma.ActionEnrollment.ActionId,
            ModuleId = ma.ModuleTeaching.ModuleId,
            StudentPersonId = ma.ActionEnrollment.Student.PersonId,
            TeacherPersonId = ma.ModuleTeaching.Teacher.PersonId,
            ModuleName = ma.ModuleTeaching.Module.Name,
            StudentName = ma.ActionEnrollment.Student.Person.FullName,
            TeacherName = ma.ModuleTeaching.Teacher.Person.FullName,
            Grade = ma.Grade
        };
    }
}
