using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using NERBABO.ApiService.Core.Account.Models;

namespace NERBABO.ApiService.Core.SessionParticipations.Dtos;

public class UpsertSessionAttendanceDto
{
    [Required(ErrorMessage = "Sessão é um campo obrigatório.")]
    public long SessionId { get; set; }

    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public List<StudentAttendanceDto> StudentAttendances { get; set; } = [];
}

public class StudentAttendanceDto
{
    [Required(ErrorMessage = "Inscrição na Ação é um campo obrigatório.")]
    public long ActionEnrollmentId { get; set; }
    
    [Required(ErrorMessage = "Nome do Formando é um campo obrigatório.")]
    public string StudentName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Presença é um campo obrigatório.")]
    public string Presence { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Frequência é um campo obrigatório.")]
    [Range(0, 24)]
    public double Attendance { get; set; }
}