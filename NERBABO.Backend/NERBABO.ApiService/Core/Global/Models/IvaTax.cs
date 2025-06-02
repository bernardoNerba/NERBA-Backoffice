namespace NERBABO.ApiService.Core.Global.Models;

public class IvaTax
{
    public IvaTax(int id, string name, int valuePercent)
    {
        Id = id;
        Name = name;
        ValuePercent = valuePercent;
    }

    public IvaTax(string name, int valuePercent)
    {
        Name = name;
        ValuePercent = valuePercent;
    }

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ValuePercent { get; set; }
    public float ValueDecimal => (float)ValuePercent / 100;

    public GeneralInfo? GeneralInfo { get; set; }
}
