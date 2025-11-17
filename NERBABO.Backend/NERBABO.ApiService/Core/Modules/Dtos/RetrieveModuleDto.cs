namespace NERBABO.ApiService.Core.Modules.Dtos
{
    public class RetrieveModuleDto
    {
        public long Id { get; set; }
        public required string Name { get; set; }
        public float Hours { get; set; }
        public bool IsActive { get; set; }
        public int CoursesQnt { get; set; }
        public required long Category { get; set; }
        public required string CategoryName { get; set; }
        public required string CategoryShortenName { get; set; }
    }
}
