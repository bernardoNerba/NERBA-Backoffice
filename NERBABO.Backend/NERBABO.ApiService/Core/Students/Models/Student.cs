using NERBABO.ApiService.Core.Companies.Models;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Shared.Models;
using System.Reflection;

namespace NERBABO.ApiService.Core.Students.Models
{
    public class Student: Entity
    {
        public long PersonId { get; set; }
        public long? CompanyId { get; set; }
        public bool IsEmployeed { get; set; }
        public bool IsRegisteredWithJobCenter { get; set; }
        public string? CompanyRole { get; set; }
        
        public required Person Person { get; set; }
        public Company? Company { get; set; }


    }
}
