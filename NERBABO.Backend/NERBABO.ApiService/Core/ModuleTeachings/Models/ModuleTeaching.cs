using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Courses.Models;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Core.ModuleAvaliations.Models;
using NERBABO.ApiService.Core.Modules.Models;
using NERBABO.ApiService.Core.ModuleTeachings.Dtos;
using NERBABO.ApiService.Core.Sessions.Dtos;
using NERBABO.ApiService.Core.Sessions.Models;
using NERBABO.ApiService.Core.Teachers.Models;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.ModuleTeachings.Models
{
    public class ModuleTeaching : Entity<long>
    {
        public long TeacherId { get; set; }
        public long ActionId { get; set; }
        public long ModuleId { get; set; }
        public float AvaliationCoordenator { get; set; }
        public float AvaliationStudents { get; set; }
        public double PaymentTotal { get; set; }
        public DateOnly? PaymentDate { get; set; }

        // Navigation properties
        public required Teacher Teacher { get; set; }
        public required CourseAction Action { get; set; }
        public required Module Module { get; set; }
        public List<Session> Sessions { get; set; } = [];
        public List<ModuleAvaliation> Avaliations = [];

        // Calculated properties
        public float AvaliationAvg =>
            (AvaliationCoordenator + AvaliationStudents) / 2;
        public bool PaymentProcessed =>
            PaymentDate.HasValue;

        public double ScheduledSessionsTime =>
            Sessions.Sum(s => s.DurationHours);

        public bool IsModuleHoursScheduled =>
            ScheduledSessionsTime == Module.Hours;

        public bool IsPayed => PaymentDate != null;

        public double CalculatedTotal(double hourRate)
        {
            return Math.Round(Sessions
                .Where(s => s.TeacherPresence.Equals(PresenceEnum.Present))
                .Sum(s => s.DurationHours)
                * hourRate, 2);
        }

        public double ScheduledPercent()
        {
            if (Module.Hours != 0)
                return Math.Round((ScheduledSessionsTime * 100) / Module.Hours, 2);

            return 0;
        }

        public static RetrieveModuleTeachingDto ConvertEntityToRetrieveDto(ModuleTeaching mt)
        {
            return new RetrieveModuleTeachingDto
            {
                Id = mt.Id,
                ActionId = mt.ActionId,
                ModuleId = mt.ModuleId,
                TeacherId = mt.TeacherId,
                AvaliationCoordenator = mt.AvaliationCoordenator,
                AvaliationStudents = mt.AvaliationStudents,
                AvaliationAvg = mt.AvaliationAvg,
                PaymentTotal = mt.PaymentTotal,
                PaymentDate = mt.PaymentDate?.ToString("dd/MM/yyyy") ?? "",
                PaymentProcessed = mt.PaymentProcessed
            };
        }

        public static ModuleTeaching ConvertCreateDtoToEntity(CreateModuleTeachingDto mt,
            Teacher teacher, Module module, CourseAction action)
        {
            return new ModuleTeaching
            {
                TeacherId = mt.TeacherId,
                ModuleId = mt.ModuleId,
                ActionId = mt.ActionId,
                Teacher = teacher,
                Action = action,
                Module = module
            };
        }

        public static MinimalModuleTeachingDto ConvertEntityToMinimalDto(ModuleTeaching mt)
        {
            return new MinimalModuleTeachingDto
            {
                ModuleTeachingId = mt.Id,
                ModuleName = mt.Module.Name,
                ScheduledPercent = mt.ScheduledPercent()
            };
        }

        public static ProcessModuleTeachingPaymentDto ConvertEntityToProcessPaymentDto(ModuleTeaching mt, GeneralInfo info)
        {
            return new ProcessModuleTeachingPaymentDto
            {
                ModuleId = mt.ModuleId,
                ModuleTeacherName = $"{mt.Teacher.Person.FullName} ({mt.Module.Name})",
                PaymentTotal = mt.PaymentTotal,
                CalculatedTotal = mt.CalculatedTotal(info.HourValueTeacher),
                PaymentDate = mt.PaymentDate?.ToString("yyyy-MM-dd") ?? "",
                IsPayed = mt.IsPayed
            };
        }
    }
}
