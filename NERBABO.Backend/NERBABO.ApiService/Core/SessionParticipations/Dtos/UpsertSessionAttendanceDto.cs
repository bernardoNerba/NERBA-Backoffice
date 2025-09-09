using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Core.SessionParticipations.Dtos;

public class UpsertSessionAttendanceDto
{
    [Required]
    public long SessionId { get; set; }
    
    [Required]
    public List<StudentAttendanceDto> StudentAttendances { get; set; } = [];
}

public class StudentAttendanceDto
{
    [Required]
    public long ActionEnrollmentId { get; set; }
    
    [Required]
    public string StudentName { get; set; } = string.Empty;
    
    [Required]
    public string Presence { get; set; } = string.Empty;
    
    [Required]
    [Range(0, 24)]
    public double Attendance { get; set; }
}