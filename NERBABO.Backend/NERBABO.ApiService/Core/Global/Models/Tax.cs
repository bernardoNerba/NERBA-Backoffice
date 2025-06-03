using Humanizer;
using NERBABO.ApiService.Core.Global.Dtos;
using NERBABO.ApiService.Core.Teachers.Models;
using NERBABO.ApiService.Shared.Enums;

namespace NERBABO.ApiService.Core.Global.Models;

public class Tax
{
    public Tax() { }
    public Tax(int id, string name, int valuePercent, TaxEnum type)
    {
        Id = id;
        Name = name;
        ValuePercent = valuePercent;
        Type = type;
    }

    public Tax(string name, int valuePercent, bool active, TaxEnum type)
    {
        Name = name;
        ValuePercent = valuePercent;
        IsActive = active;
        Type = type;
    }

    public Tax(int id, string name, int valuePercent, bool active, TaxEnum type)
    {
        Id = id;
        Name = name;
        ValuePercent = valuePercent;
        IsActive = active;
        Type = type;
    }

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ValuePercent { get; set; }
    public float ValueDecimal => (float)ValuePercent / 100;
    public bool IsActive { get; set; }
    public TaxEnum Type { get; set; } = TaxEnum.IVA;

    public GeneralInfo? GeneralInfo { get; set; }
    public List<Teacher> IvaTeachers { get; set; } = [];
    public List<Teacher> IrsTeachers { get; set; } = [];

    public static RetrieveTaxDto ConvertEntityToRetrieveDto(Tax tax)
    {
        return new RetrieveTaxDto()
        {
            Id = tax.Id,
            Name = tax.Name,
            ValuePercent = tax.ValuePercent,
            IsActive = tax.IsActive,
            Type = tax.Type.Humanize(),
        };
    }

    public static Tax ConvertCreateDtoToEntity(CreateTaxDto tax)
    {
        return new Tax(tax.Name, tax.ValuePercent, true, tax.Type.DehumanizeTo<TaxEnum>());
    }

    public static Tax ConvertUpdateDtoToEntity(UpdateTaxDto tax)
    {
        return new Tax(tax.Id, tax.Name, tax.ValuePercent, tax.IsActive, tax.Type.DehumanizeTo<TaxEnum>());
    }

}
