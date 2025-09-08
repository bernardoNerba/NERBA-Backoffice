namespace NERBABO.ApiService.Core.Courses.Dtos;

public class KpisCourseDto
{
    public int TotalStudents { get; set; }
    public int TotalApproved { get; set; }
    public double TotalVolumeHours { get; set; }
    public double TotalVolumeDays { get; set; }
}