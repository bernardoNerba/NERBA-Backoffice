using System;

namespace NERBABO.ApiService.Core.ModuleTeachings.Dtos;

public class RetrieveModuleTeachingDto
{
    public long Id { get; set; }
    public long ActionId { get; set; }
    public long ModuleId { get; set; }
    public long TeacherId { get; set; }
    public float AvaliationCoordenator { get; set; }
    public float AvaliationStudents { get; set; }
    public float AvaliationAvg { get; set; }
    public double PaymentTotal { get; set; }
    public string PaymentDate { get; set; } = string.Empty;
    public bool PaymentProcessed { get; set; }

}
