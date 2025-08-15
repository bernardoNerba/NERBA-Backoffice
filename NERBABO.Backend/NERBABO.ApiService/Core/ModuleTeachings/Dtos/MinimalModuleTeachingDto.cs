namespace NERBABO.ApiService.Core.Sessions.Dtos;

public class MinimalModuleTeachingDto
{
    public long ModuleTeachingId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public double ScheduledPercent { get; set; }
}