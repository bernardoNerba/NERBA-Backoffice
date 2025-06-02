using NERBABO.ApiService.Core.Global.Dtos;

namespace NERBABO.ApiService.Core.Global.Models;

public class IvaTax
{
    public IvaTax() { }
    public IvaTax(int id, string name, int valuePercent)
    {
        Id = id;
        Name = name;
        ValuePercent = valuePercent;
    }

    public IvaTax(string name, int valuePercent, bool active)
    {
        Name = name;
        ValuePercent = valuePercent;
        IsActive = active;
    }

    public IvaTax(int id, string name, int valuePercent, bool active)
    {
        Id = id;
        Name = name;
        ValuePercent = valuePercent;
        IsActive = active;
    }

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ValuePercent { get; set; }
    public float ValueDecimal => (float)ValuePercent / 100;
    public bool IsActive { get; set; }

    public GeneralInfo? GeneralInfo { get; set; }

    public static RetrieveIvaTaxDto ConvertEntityToRetrieveDto(IvaTax tax)
    {
        return new RetrieveIvaTaxDto()
        {
            Id = tax.Id,
            Name = tax.Name,
            ValuePercent = tax.ValuePercent,
            IsActive = tax.IsActive
        };
    }

    public static IvaTax ConvertCreateDtoToEntity(CreateIvaTaxDto tax)
    {
        return new IvaTax(tax.Name, tax.ValuePercent, true);
    }

    public static IvaTax ConvertUpdateDtoToEntity(UpdateIvaTaxDto tax)
    {
        return new IvaTax(tax.Id, tax.Name, tax.ValuePercent, tax.IsActive);
    }

}
