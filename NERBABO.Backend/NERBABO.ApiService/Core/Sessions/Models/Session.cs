using Humanizer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NERBABO.ApiService.Core.ModuleTeachings.Models;
using NERBABO.ApiService.Core.SessionParticipations.Models;
using NERBABO.ApiService.Core.Sessions.Dtos;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Sessions.Models;

public class Session : Entity<long>
{
    public long ModuleTeachingId { get; set; }
    public WeekDaysEnum Weekday { get; set; }
    public DateOnly ScheduledDate { get; set; }
    public TimeOnly Start { get; set; }
    public double DurationHours { get; set; }
    public PresenceEnum TeacherPresence { get; set; } = PresenceEnum.Unknown;
    public string Note { get; set; } = string.Empty;

    // Navigation properties
    public required ModuleTeaching ModuleTeaching { get; set; }
    public List<SessionParticipation> Participants { get; set; } = [];

    // Calculated properties
    public TimeOnly End =>
        Start.AddHours(DurationHours);

    public string Time => $"{Start:HH:mm} - {End:HH:mm}";

    public static RetrieveSessionDto ConvertEntityToRetrieveDto(Session s)
    {
        return new RetrieveSessionDto
        {
            Id = s.Id,
            ModuleTeachingId = s.ModuleTeachingId,
            ModuleId = s.ModuleTeaching.ModuleId,
            ModuleName = s.ModuleTeaching.Module.Name,
            TeacherPersonId = s.ModuleTeaching.Teacher.PersonId,
            TeacherPersonName = s.ModuleTeaching.Teacher.Person.FullName,
            CoordenatorPersonId = s.ModuleTeaching.Action.Coordenator.PersonId,
            CoordenatorPersonName = s.ModuleTeaching.Action.Coordenator.Person?.FullName ?? "",
            ScheduledDate = s.ScheduledDate.ToString("yyyy-MM-dd"),
            Weekday = s.Weekday.Humanize(LetterCasing.Sentence),
            Time = s.Time,
            DurationHours = s.DurationHours,
            TeacherPresence = s.TeacherPresence.Humanize(),
            Note = s.Note
        };
    }

    public static Session ConvertCreateDtoToEntity(CreateSessionDto s, ModuleTeaching mt)
    {
        return new Session
        {
            ModuleTeachingId = mt.Id,
            Weekday = s.Weekday.DehumanizeTo<WeekDaysEnum>(),
            ScheduledDate = DateOnly.Parse(s.ScheduledDate),
            Start = TimeOnly.Parse(s.Start),
            DurationHours = s.DurationHours,
            ModuleTeaching = mt,
            Note = s.Note,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}