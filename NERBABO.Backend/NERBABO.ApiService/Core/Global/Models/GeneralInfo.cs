using NERBABO.ApiService.Core.Global.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Global.Models;

public class GeneralInfo : Entity<long>
{
    public string Designation { get; set; } = string.Empty;
    public int? IvaId { get; set; }
    public string Site { get; set; } = string.Empty;
    public float HourValueTeacher { get; set; }
    public float HourValueAlimentation { get; set; }
    public string BankEntity { get; set; } = string.Empty;
    public string Iban { get; set; } = string.Empty;
    public string Nipc { get; set; } = string.Empty;
    public string? Logo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string InsurancePolicy { get; set; } = string.Empty;
    public string FacilitiesCharacterization { get; set; } = string.Empty;

    public Tax? IvaTax { get; set; }

    public static RetrieveGeneralInfoDto ConvertEntityToRetrieveDto(GeneralInfo generalInfo)
    {
        return new RetrieveGeneralInfoDto
        {
            Designation = generalInfo.Designation,
            Site = generalInfo.Site,
            HourValueTeacher = generalInfo.HourValueTeacher,
            HourValueAlimentation = generalInfo.HourValueAlimentation,
            BankEntity = generalInfo.BankEntity,
            Iban = generalInfo.Iban,
            Nipc = generalInfo.Nipc,
            LogoUrl = !string.IsNullOrEmpty(generalInfo.Logo)
                ? $"{Helper.UrlHelper.GetBaseUrl()}/uploads/images/{generalInfo.Logo.Replace('\\', '/')}"
                : null,
            IvaId = generalInfo.IvaTax?.Id ?? 0,
            IvaRegime = generalInfo.IvaTax?.Name ?? string.Empty,
            IvaPercent = generalInfo.IvaTax?.ValuePercent ?? 0,
            Email = generalInfo.Email,
            Slug = generalInfo.Slug,
            PhoneNumber = generalInfo.PhoneNumber,
            Website = generalInfo.Website
        };
    }
}
