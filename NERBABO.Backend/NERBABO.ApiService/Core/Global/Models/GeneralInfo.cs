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

}
