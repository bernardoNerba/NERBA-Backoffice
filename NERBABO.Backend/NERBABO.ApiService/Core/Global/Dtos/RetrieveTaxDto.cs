namespace NERBABO.ApiService.Core.Global.Dtos;

public class RetrieveTaxDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public float ValuePercent { get; set; }
    public float ValueDecimal => (float)ValuePercent / 100;
    public bool IsActive { get; set; }
    public string Type { get; set; } = string.Empty;

}
