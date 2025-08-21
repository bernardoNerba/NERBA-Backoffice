
namespace NERBABO.ApiService.Core.Frames.Dtos;

public class RetrieveFrameDto
{
    public long Id { get; set; }
    public string Program { get; set; } = string.Empty;
    public string Intervention { get; set; } = string.Empty;
    public string InterventionType { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public string? ProgramLogoUrl { get; set; }
    public string? FinancementLogoUrl { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}
