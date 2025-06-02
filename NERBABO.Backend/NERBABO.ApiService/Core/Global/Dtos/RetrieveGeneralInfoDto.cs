namespace NERBABO.ApiService.Core.Global.Dtos;

public class RetrieveGeneralInfoDto
{
    public string Designation { get; set; } = string.Empty;
    public string Site { get; set; } = string.Empty;
    public float HourValueTeacher { get; set; }
    public float HourValueAlimentation { get; set; }
    public string BankEntity { get; set; } = string.Empty;
    public string Iban { get; set; } = string.Empty;
    public string Nipc { get; set; } = string.Empty;
    public string LogoFinancing { get; set; } = string.Empty;
    public int IvaId { get; set; }
    public int IvaPercent { get; set; }
    public string IvaRegime { get; set; } = string.Empty;

}
