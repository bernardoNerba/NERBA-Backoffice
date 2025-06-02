namespace NERBABO.ApiService.Core.Global.Dtos;

public class RetrieveIvaTaxDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ValuePercent { get; set; }
    public float ValueDecimal => (float)ValuePercent / 100;
    public bool IsActive { get; set; }

}
