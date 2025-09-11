namespace NERBABO.ApiService.Core.ModuleAvaliations.Dtos;

public class RetrieveModuleAvaliationDto
{
    public long Id { get; set; }
    public long ActionId { get; set; }
    public long ModuleId { get; set; }
    public long StudentPersonId { get; set; }
    public long TeacherPersonId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public int Grade { get; set; }
    public bool Evaluated { get; set; }
}