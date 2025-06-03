namespace NERBABO.ApiService.Core.Teachers.Dtos;

public class RetrieveTeacherDto
{
    public long Id { get; set; }
    public string IvaRegime { get; set; } = string.Empty;
    public string IrsRegime { get; set; } = string.Empty;
    public string Ccp { get; set; } = string.Empty;
    public string Competences { get; set; } = string.Empty;
    public float AvarageRating { get; set; } = 0.0f;
    public bool IsActive { get; set; }

}
