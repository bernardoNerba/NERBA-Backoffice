namespace NERBABO.ApiService.Core.ModuleAvaliations.Dtos;

public class AvaliationsByModuleDto
{
    public long ActionId { get; set; }
    public long ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public long TeacherPersonId { get; set; }
    public string TeacherName { get; set; } = string.Empty;

    public List<GradingInfoDto> Gradings { get; set; } = [];
}

public class GradingInfoDto
{
    public long StudentPersonId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int Grade { get; set; }
    public bool Evaluated { get; set; }
}
