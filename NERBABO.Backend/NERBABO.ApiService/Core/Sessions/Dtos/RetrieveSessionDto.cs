using System;

namespace NERBABO.ApiService.Core.Sessions.Dtos;

public class RetrieveSessionDto
{
    public long Id { get; set; }
    public long ModuleTeachingId { get; set; }
    public long ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;

    public long TeacherPersonId { get; set; }
    public string TeacherPersonName { get; set; } = string.Empty;

    public long CoordenatorPersonId { get; set; }
    public string CoordenatorPersonName { get; set; } = string.Empty;

    public string ScheduledDate { get; set; } = string.Empty;
    public string Weekday { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public double DurationHours { get; set; }
    public string TeacherPresence { get; set; } = string.Empty;

}
