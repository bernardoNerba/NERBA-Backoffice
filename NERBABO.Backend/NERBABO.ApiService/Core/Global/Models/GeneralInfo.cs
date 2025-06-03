using NERBABO.ApiService.Core.Global.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Global.Models;

public class GeneralInfo : Entity
{
    public string Designation { get; set; } = string.Empty;
    public int? IvaId { get; set; }
    public string Site { get; set; } = string.Empty;
    public float HourValueTeacher { get; set; }
    public float HourValueAlimentation { get; set; }
    public string BankEntity { get; set; } = string.Empty;
    public string Iban { get; set; } = string.Empty;
    public string Nipc { get; set; } = string.Empty;
    public string LogoFinancing { get; set; } = string.Empty;

    public IvaTax? IvaTax { get; set; }

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
            LogoFinancing = generalInfo.LogoFinancing,
            IvaId = generalInfo.IvaTax?.Id ?? 0,
            IvaRegime = generalInfo.IvaTax?.Name ?? string.Empty,
            IvaPercent = generalInfo.IvaTax?.ValuePercent ?? 0
        };
    }

    public static GeneralInfo ConvertUpdateDtoToEntity(UpdateGeneralInfoDto updateGeneralInfo)
    {
        return new GeneralInfo
        {
            Id = 1,
            Designation = updateGeneralInfo.Designation,
            Site = updateGeneralInfo.Site,
            HourValueTeacher = updateGeneralInfo.HourValueTeacher,
            HourValueAlimentation = updateGeneralInfo.HourValueAlimentation,
            BankEntity = updateGeneralInfo.BankEntity,
            Iban = updateGeneralInfo.Iban,
            Nipc = updateGeneralInfo.Nipc,
            LogoFinancing = updateGeneralInfo.LogoFinancing,
            IvaId = updateGeneralInfo.IvaId,
            UpdatedAt = DateTime.UtcNow
        };
    }

}
