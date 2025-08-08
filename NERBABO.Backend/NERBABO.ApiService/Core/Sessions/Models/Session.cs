using Humanizer;
using NERBABO.ApiService.Core.ModuleTeachings.Models;
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

    // Navigation properties
    public required ModuleTeaching ModuleTeaching { get; set; }

    // Calculated properties
    public TimeOnly End =>
        Start.AddHours(DurationHours);

    public string Time => $"{Start.ToString("HH:mm")} - {End.ToString("HH:mm")}";


    public static RetrieveSessionDto ConvertEntityToRetrieveDto(Session s)
    {
        return new RetrieveSessionDto
        {
            Id = s.Id,
            ModuleTeachingId = s.ModuleTeachingId,
            ModuleId = s.ModuleTeaching.ModuleId,
            ModuleName = s.ModuleTeaching.Module.Name,
            PersonId = s.ModuleTeaching.Teacher.PersonId,
            PersonName = $"{s.ModuleTeaching.Teacher.Person.FirstName} {s.ModuleTeaching.Teacher.Person.LastName}",
            ScheduledDate = s.ScheduledDate.ToString("mm-dd-yyyy"),
            Time = s.Time,
            DurationHours = s.DurationHours,
            TeacherPresence = s.TeacherPresence.Humanize()

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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}