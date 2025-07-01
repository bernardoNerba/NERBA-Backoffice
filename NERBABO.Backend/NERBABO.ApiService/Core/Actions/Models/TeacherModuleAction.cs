using NERBABO.ApiService.Core.Modules.Models;
using NERBABO.ApiService.Core.Teachers.Models;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Actions.Models
{
    public class TeacherModuleAction : Entity<long>
    {
        public long TeacherId { get; set; }
        public long ActionId { get; set; }
        public long ModuleId { get; set; }
        public float AvaliationNerba { get; set; }
        public float AvaliationStudents { get; set; }
        public double TotalPayment { get; set; }
        public DateOnly? PaymentDate { get; set; }

        // Navigation properties
        public required Teacher Teacher { get; set; }
        public required CourseAction Action { get; set; }
        public required Module Module { get; set; }

        // Calculated properties
        public float AvaliationAvg => 
            (AvaliationNerba + AvaliationStudents) / 2;
        public bool PaymentProcessed =>
            PaymentDate.HasValue;

    }
}
