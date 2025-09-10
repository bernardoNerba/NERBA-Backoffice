using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Core.SessionParticipations.Dtos;

public class UpdateSessionParticipationDto
{
    [Required(ErrorMessage = "Id da participação é um campo obrigatório")]
    public long SessionParticipationId { get; set; }
    
    [Required(ErrorMessage = "Sessão é um campo obrigatório.")]
    public long SessionId { get; set; }
    
    [Required(ErrorMessage = "Inscrição na Ação é um campo obrigatório")]
    public long ActionEnrollmentId { get; set; }
    
    [Required(ErrorMessage = "Presença é um campo obrigatório.")]
    public string Presence { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Frequência é um campo obrigatório.")]
    [Range(0, 24)]
    public double Attendance { get; set; }
}