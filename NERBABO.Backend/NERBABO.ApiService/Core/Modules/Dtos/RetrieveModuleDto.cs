namespace NERBABO.ApiService.Core.Modules.Dtos
{
    public class RetrieveModuleDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public float Hours { get; set; }
        public bool IsActive { get; set; }
        public int CoursesQnt { get; set; }
    }
}
