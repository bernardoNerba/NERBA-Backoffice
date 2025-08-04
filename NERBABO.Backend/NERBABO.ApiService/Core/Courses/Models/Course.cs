using Humanizer;
using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Courses.Dtos;
using NERBABO.ApiService.Core.Frames.Models;
using NERBABO.ApiService.Core.Modules.Models;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;
using System.Globalization;

namespace NERBABO.ApiService.Core.Courses.Models
{
    public class Course : Entity<long>
    {

        // Entity Properties
        public long FrameId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Objectives { get; set; } = string.Empty;
        public float TotalDuration { get; set; }
        public StatusEnum Status { get; set; }
        public string Area { get; set; } = string.Empty;
        public HabilitationEnum MinHabilitationLevel { get; set; } = HabilitationEnum.WithoutProof;
        public List<DestinatorTypeEnum> Destinators { get; set; } = [];

        // Calculated Properties
        public float CurrentDuration =>
            Modules.Sum(m => m.Hours);

        public float RemainingDuration =>
            TotalDuration - CurrentDuration;

        public bool IsCourseActive =>
            Status == StatusEnum.NotStarted || Status == StatusEnum.InProgress;

        public List<string> FormattedModuleNames =>
            [.. Modules.Select(m => $"{m.Name} ({m.Hours} horas)")];

        public int ActionsQnt => Actions.Count;
        public int ModulesQnt => Modules.Count;

        // Navigation Properties
        public Frame Frame { get; set; }
        public List<Module> Modules { get; set; } = [];
        public List<CourseAction> Actions { get; set; } = [];


        // Constructors
        public Course()
        {
            Frame = new Frame();
        }

        public Course(long frameId, string title, string objectives,
            List<DestinatorTypeEnum> destinators, float totalDuration, 
            StatusEnum status, string area, Frame frame, 
            HabilitationEnum minHabilitationLevel)
        {
            FrameId = frameId;
            Title = title;
            Objectives = objectives;
            Destinators = destinators;
            TotalDuration = totalDuration;
            Status = status;
            Area = area;
            Frame = frame;
            MinHabilitationLevel = minHabilitationLevel;
        }

        // Convert Methods
        public static RetrieveCourseDto ConvertEntityToRetrieveDto(Course c)
        {
            return new RetrieveCourseDto
            {
                Id = c.Id,
                FrameId = c.FrameId,
                FrameProgram = c.Frame.Program,
                Title = c.Title,
                Objectives = c.Objectives,
                Destinators = [.. c.Destinators.Select(c => c.Humanize().Transform(To.TitleCase))],
                TotalDuration = c.TotalDuration,
                Status = c.Status.Humanize().Transform(To.TitleCase),
                Area = c.Area,
                MinHabilitationLevel = c.MinHabilitationLevel.Humanize().Transform(To.TitleCase),
                CreatedAt = c.CreatedAt.Humanize(culture: new CultureInfo("pt-PT")),
                Modules = [.. c.Modules.Select(Module.ConvertEntityToRetrieveDto)],
                RemainingDuration = c.RemainingDuration,
                ModulesQnt = c.ModulesQnt,
                ActionsQnt = c.ActionsQnt
            };
        }

        public static Course ConvertCreateDtoToEntity(CreateCourseDto c, Frame f, List<Module> ms)
        {
            return new Course
            (
                c.FrameId,
                c.Title,
                c.Objectives ?? "",
                [.. c.Destinators?.Select(d => d.DehumanizeTo<DestinatorTypeEnum>()) ?? []],
                c.TotalDuration,
                c.Status.DehumanizeTo<StatusEnum>(),
                c.Area ?? "",
                f,
                c.MinHabilitationLevel?.DehumanizeTo<HabilitationEnum>()
                    ?? HabilitationEnum.WithoutProof
            )
            {
                Modules = ms,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public bool CanAddModule(float moduleHours)
        {
            // Check if adding the module would exceed the total duration
            return CurrentDuration + moduleHours <= TotalDuration;
        }

        public static bool CanAddModule(float currentDuration, float moduleHours, float totalDuration)
        {
            return currentDuration + moduleHours <= totalDuration;
        }

        public bool IsModuleAlreadyAssigned(long moduleId)
        {
            // Check if the module is already assigned to the course
            return Modules.Any(m => m.Id == moduleId);
        }
   
    }
}
