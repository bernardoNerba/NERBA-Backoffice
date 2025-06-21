using NERBABO.ApiService.Shared.Enums;
using System.Text.Json.Serialization;

namespace NERBABO.ApiService.Core.Courses.Dtos
{
    public class RetrieveCourseDto
    {
        public long Id { get; set; }
        public long FrameId { get; set; }
        public string FrameProgram { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Objectives { get; set; } = string.Empty;
        public string Destinators { get; set; } = string.Empty;
        public float TotalDuration { get; set; }
        public string CourseStatus { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string MinHabilitationLevel { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }
}
