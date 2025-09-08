using System;

namespace NERBABO.ApiService.Core.SessionParticipations.Dtos;

public class RetrieveSessionParticipationDto
{
    public long SessionParticipationId { get; set; }
    public long SessionId { get; set; }
    public long ActionEnrollmentId { get; set; }
    public string Presence { get; set; } = string.Empty;
    public double Attendance { get; set; }
}
