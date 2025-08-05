using Humanizer;
using NERBABO.ApiService.Core.Global.Dtos;
using NERBABO.ApiService.Core.Teachers.Models;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Global.Models;

public class Tax : Entity<int>
{
    public string Name { get; set; } = string.Empty;
    public float ValuePercent { get; set; }
    public float ValueDecimal => ValuePercent / 100;
    public bool IsActive { get; set; }
    public TaxEnum Type { get; set; } = TaxEnum.IVA;

    // Navigation properties
    public GeneralInfo? GeneralInfo { get; set; }
    public List<Teacher> IvaTeachers { get; set; } = [];
    public List<Teacher> IrsTeachers { get; set; } = [];

    // constructors
    public Tax() { }
    public Tax(int id, string name, float valuePercent, TaxEnum type)
    {
        Id = id;
        Name = name;
        ValuePercent = valuePercent;
        Type = type;
    }

    public Tax(string name, float valuePercent, bool active, TaxEnum type)
    {
        Name = name;
        ValuePercent = valuePercent;
        IsActive = active;
        Type = type;
    }

    public Tax(int id, string name, float valuePercent, bool active, TaxEnum type)
    {
        Id = id;
        Name = name;
        ValuePercent = valuePercent;
        IsActive = active;
        Type = type;
    }

    // convertion and validation helper class methods
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

    public static bool IsValidTaxType(string type)
    {
        return type.Equals(TaxEnum.IVA.Humanize(), StringComparison.OrdinalIgnoreCase) ||
               type.Equals(TaxEnum.IRS.Humanize(), StringComparison.OrdinalIgnoreCase);
    }
}
