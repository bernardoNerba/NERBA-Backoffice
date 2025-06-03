using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Teachers.Models;

public class Teacher : Entity
{
    public int IvaRegimeId { get; set; }
    public int IrsRegimeId { get; set; }
    public long PersonId { get; set; }
    public string Ccp { get; set; } = string.Empty;
    public string Competences { get; set; } = string.Empty;
    public float AvarageRating { get; set; } = 0.0f;
    public bool IsActive { get; set; }



    public Tax? IvaRegime { get; set; }
    public Tax? IrsRegime { get; set; }
    public required Person Person { get; set; }
    public string CommaSeparatedCompetences => string.Join(", ", Competences);






}
