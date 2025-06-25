using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Courses.Models;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Actions.Models
{
    public class CourseAction : Entity<long>
    {
        public long CourseId { get; set; }
        public string CoordenatorId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string AdministrationCode { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public string Locality { get; set; } = string.Empty;
        public List<WeekDaysEnum> WeekDays { get; set; } = [];
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public StatusEnum Status { get; set; } = StatusEnum.NotStarted;
        public RegimentTypeEnum Regiment { get; set; } = RegimentTypeEnum.hybrid;


        // Calculated properties
        public bool AllPaymentsProcessed
            => TeacherModuleActions.All(a => a.PaymentProcessed);

        public bool AllModulesOfActionHaveTeacher
            => Course.Modules.All(m => 
                TeacherModuleActions.Any(tma => tma.ModuleId == m.Id));
        //{
        //    foreach(var module in Course.Modules)
        //    {
        //        if (!TeacherModuleActions.Any(tma => tma.ModuleId == module.Id))
        //            return false;
        //    }
        //    return true;
        //}


        // Navigation properties
        public required Course Course { get; set; }
        public required User Coordenator { get; set; }
        //public List<Student> Students { get; set; } = [];
        public List<TeacherModuleAction> TeacherModuleActions { get; set; } = [];

    }
}
