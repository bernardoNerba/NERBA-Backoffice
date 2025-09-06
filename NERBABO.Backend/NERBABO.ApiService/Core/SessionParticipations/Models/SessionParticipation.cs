using Humanizer;
using NERBABO.ApiService.Core.Enrollments.Models;
using NERBABO.ApiService.Core.SessionParticipations.Dtos;
using NERBABO.ApiService.Core.Sessions.Models;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.SessionParticipations.Models;

public class SessionParticipation : Entity<long>
{
    public long SessionId { get; set; }
    public long ActionEnrollmentId { get; set; }
    public PresenceEnum Presence { get; set; } = PresenceEnum.Unknown;
    public double Attendance { get; set; }


    // Navigation Properties
    public required Session Session { get; set; }
    public required ActionEnrollment ActionEnrollment { get; set; }

    public static RetrieveSessionParticipationDto ConvertEntityToRetrieveDto(SessionParticipation sp)
    {
        return new RetrieveSessionParticipationDto
        {
            SessionParticipationId = sp.Id,
            SessionId = sp.SessionId,
            ActionEnrollmentId = sp.ActionEnrollmentId,
            Presence = sp.Presence.Humanize(LetterCasing.Title),
            Attendance = sp.Attendance
        };
        
    }
}
