using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Core.SessionParticipations.Dtos;

public class UpdateSessionParticipationDto
{
    [Required]
    public long SessionParticipationId { get; set; }
    
    [Required]
    public long SessionId { get; set; }
    
    [Required]
    public long ActionEnrollmentId { get; set; }
    
    [Required]
    public string Presence { get; set; } = string.Empty;
    
    [Required]
    [Range(0, 24)]
    public double Attendance { get; set; }
}