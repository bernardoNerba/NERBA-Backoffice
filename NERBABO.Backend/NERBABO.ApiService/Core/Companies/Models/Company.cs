using NERBABO.ApiService.Core.Students.Models;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Companies.Models
{
    public class Company: Entity
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Locality { get; set; }
        public string? ZipCode { get; set; }
        public string? Email { get; set; }
        public AtivitySectorEnum AtivitySector { get; set; } = AtivitySectorEnum.Unknown;
        public CompanySizeEnum Size { get; set; } = CompanySizeEnum.Micro;


        public List<Student> Students { get; set; } = [];
    }
}
