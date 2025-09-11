using Humanizer;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Actions.Dtos;
using NERBABO.ApiService.Core.Courses.Models;
using NERBABO.ApiService.Core.Enrollments.Models;
using NERBABO.ApiService.Core.ModuleTeachings.Models;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Actions.Models
{
    public class CourseAction : Entity<long>
    {
        public long CourseId { get; set; }
        public string CoordenatorId { get; set; } = string.Empty;
        public int ActionNumber { get; set; }
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
            => ModuleTeachings.All(a => a.PaymentProcessed);

        public bool AllModulesOfActionHaveTeacher
            => Course.Modules.All(m =>
                ModuleTeachings.Any(tma => tma.ModuleId == m.Id));

        public bool IsActionActive =>
            Status == StatusEnum.NotStarted || Status == StatusEnum.InProgress;


        public string AllDiferentSessionTeachers()
        {
            var teachers = ModuleTeachings.Select(mt => mt.Teacher.Person.FirstName).ToHashSet();
            return String.Join(" | ", teachers);
        }

        public string AllDiferentSessionTimes()
        {
            var times = ModuleTeachings.SelectMany(mt => mt.Sessions.Select(s => s.Time)).ToHashSet();
            return String.Join(" | ", times);
        }

        public bool AllSessionsScheduled =>
            ModuleTeachings.All(mt => mt.ScheduledPercent() == 100);

        public string Title => $"{ActionNumber} - {Locality}";

        public int TotalStudents => ActionEnrollments.Count;
        public int TotalApproved => ActionEnrollments.Count(ae => ae.ApprovalStatus == ApprovalStatusEnum.Approved);
        // Updated KPI calculations based on actual attendance
        public double TotalVolumeHours => ActionEnrollments
            .SelectMany(ae => ae.Participants)
            .Sum(sp => sp.Attendance);
            
        public int TotalVolumeDays => ActionEnrollments
            .SelectMany(ae => ae.Participants)
            .Count(sp => sp.Presence == PresenceEnum.Present);

        // Navigation properties
        public required Course Course { get; set; }
        public required User Coordenator { get; set; }
        public List<ModuleTeaching> ModuleTeachings { get; set; } = [];
        public List<ActionEnrollment> ActionEnrollments { get; set; } = [];

        public KpisActionDto ConvertEntityToKpiDto()
        {
            return new KpisActionDto
            {
                TotalStudents = TotalStudents,
                TotalApproved = TotalApproved,
                TotalVolumeDays = TotalVolumeDays,
                TotalVolumeHours = TotalVolumeHours
            };
        }

        public static RetrieveCourseActionDto ConvertEntityToRetrieveDto(CourseAction ca, User u, Course c)
        {
            return new RetrieveCourseActionDto
            {
                Id = ca.Id,

                CourseId = c.Id,
                CourseTitle = c.Title,
                CourseArea = c.Area,
                CourseMinHabilitationLevel = c.MinHabilitationLevel.Humanize().Transform(To.TitleCase),
                CourseTotalDuration = c.TotalDuration,
                CourseDestinators = [.. c.Destinators.Select(x => x.Humanize().Transform(To.TitleCase))],

                CoordenatorId = u.PersonId,
                CoordenatorName = u.UserName ?? "",

                ActionNumber = ca.ActionNumber,
                Title = ca.Title,
                AdministrationCode = ca.AdministrationCode,
                Address = ca.Address,
                Locality = ca.Locality,
                WeekDays = [.. ca.WeekDays.Select(x => x.Humanize().Transform(To.SentenceCase))],
                StartDate = ca.StartDate.ToString("yyyy-MM-dd"),
                EndDate = ca.EndDate.ToString("yyyy-MM-dd"),
                Status = ca.Status.Humanize().Transform(To.TitleCase),
                Regiment = ca.Regiment.Humanize().Transform(To.TitleCase)
            };
        }

        public static CourseAction ConvertCreateDtoToEntity(CreateCourseActionDto ca, User u, Course c)
        {
            return new CourseAction
            {
                CourseId = c.Id,
                ActionNumber = c.Actions.Count + 1,
                CoordenatorId = u.Id,
                AdministrationCode = ca.AdministrationCode,
                Address = ca.Address,
                Locality = ca.Locality,
                WeekDays = [.. ca.WeekDays.Select(x => x.DehumanizeTo<WeekDaysEnum>())],
                StartDate = Helper.StringDateOnlyConverter.ConvertToDateOnly(ca.StartDate) ?? new DateOnly(),
                EndDate = Helper.StringDateOnlyConverter.ConvertToDateOnly(ca.EndDate) ?? new DateOnly(),
                Status = ca.Status.DehumanizeTo<StatusEnum>(),
                Regiment = ca.Regiment.DehumanizeTo<RegimentTypeEnum>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Coordenator = u,
                Course = c
            };
        }

    }
}
