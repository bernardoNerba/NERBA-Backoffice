using NERBABO.ApiService.Core.Modules.Dtos;

namespace NERBABO.ApiService.Core.Courses.Dtos
{
    public class RetrieveCourseDto
    {
        public long Id { get; set; }
        public long FrameId { get; set; }
        public string FrameProgram { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Objectives { get; set; } = string.Empty;
        public List<string> Destinators { get; set; } = [];
        public float TotalDuration { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string MinHabilitationLevel { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public List<RetrieveModuleDto> Modules { get; set; } = [];
        public float RemainingDuration { get; set; } = 0;

        public int ModulesQnt { get; set; }
        public int ActionsQnt { get; set; }
    }
}
