using System;
using NERBABO.ApiService.Core.Enrollments.Models;
using NERBABO.ApiService.Core.ModuleTeachings.Models;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.ModuleAvaliations.Models;

public class ModuleAvaliation : Entity<long>
{
    public long ModuleTeachingId { get; set; }
    public long ActionEnrollmentId { get; set; }
    public int Grade { get; set; }
    public ModuleTeaching? ModuleTeaching { get; set; }
    public ActionEnrollment? ActionEnrollment { get; set; }
}
