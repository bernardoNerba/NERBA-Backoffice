using Humanizer;
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
        public string Destinators { get; set; } = string.Empty;
        public float TotalDuration { get; set; }
        public bool Status { get; set; }
        public string Area { get; set; } = string.Empty;
        public HabilitationEnum MinHabilitationLevel { get; set; } = HabilitationEnum.WithoutProof;

        // Calculated Properties
        public string CourseStatus => Status
            ? "Em Andamento"
            : "Concluído";

        public float CurrentDuration =>
            Modules.Sum(m => m.Hours);

        public float RemainingDuration =>
            TotalDuration - CurrentDuration;

        // Navigation Properties
        public Frame Frame { get; set; }
        public List<Module> Modules { get; set; } = [];


        // Constructors
        public Course()
        {
            Frame = new Frame();
        }

        public Course(long frameId, string title, string objectives,
            string destinators, float totalDuration, bool status, string area,
            Frame frame, HabilitationEnum minHabilitationLevel)
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
                Destinators = c.Destinators,
                TotalDuration = c.TotalDuration,
                CourseStatus = c.CourseStatus,
                Area = c.Area,
                MinHabilitationLevel = c.MinHabilitationLevel.Humanize().Transform(To.TitleCase),
                CreatedAt = c.CreatedAt.Humanize(culture: new CultureInfo("pt-PT")),
                Modules = [.. c.Modules.Select(x => Module.ConvertEntityToRetrieveDto(x))],
                RemainingDuration = c.RemainingDuration
            };
        }

        public static Course ConvertCreateDtoToEntity(CreateCourseDto c, Frame f)
        {
            return new Course
            (
                c.FrameId,
                c.Title,
                c.Objectives ?? "",
                c.Destinators ?? "",
                c.TotalDuration, true,
                c.Area ?? "",
                f,
                c.MinHabilitationLevel?.DehumanizeTo<HabilitationEnum>()
                    ?? HabilitationEnum.WithoutProof
            )
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Course ConvertUpdateDtoToEntity(UpdateCourseDto c, Frame f)
        {
            return new Course
            (
                c.FrameId,
                c.Title,
                c.Objectives ?? "",
                c.Destinators ?? "",
                c.TotalDuration, true,
                c.Area ?? "",
                f,
                c.MinHabilitationLevel?.DehumanizeTo<HabilitationEnum>()
                    ?? HabilitationEnum.WithoutProof
            )
            {
                Id = c.Id,
                Status = c.Status,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public bool CanAddModule(float moduleHours)
        {
            // Check if adding the module would exceed the total duration
            return CurrentDuration + moduleHours <= TotalDuration;
        }

        public bool IsModuleAlreadyAssigned(long moduleId)
        {
            // Check if the module is already assigned to the course
            return Modules.Any(m => m.Id == moduleId);
        }


    }
}
