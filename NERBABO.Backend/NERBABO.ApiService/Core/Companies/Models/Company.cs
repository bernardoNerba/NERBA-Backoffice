using Humanizer;
using NERBABO.ApiService.Core.Companies.Dtos;
using NERBABO.ApiService.Core.Students.Models;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Companies.Models
{
    public class Company : Entity<long>
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Locality { get; set; }
        public string? ZipCode { get; set; }
        public string? Email { get; set; }
        public AtivitySectorEnum AtivitySector { get; set; } = AtivitySectorEnum.Unknown;
        public CompanySizeEnum Size { get; set; } = CompanySizeEnum.Micro;


        // Calculated properties
        public int StudentsCount => Students.Count;

        // Navigation Properties
        public List<Student> Students { get; set; } = [];

        public static RetrieveCompanyDto ConvertEntityToRetrieveDto(Company c)
        {
            return new RetrieveCompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber,
                Locality = c.Locality,
                ZipCode = c.ZipCode,
                Email = c.Email,
                AtivitySector = c.AtivitySector.Humanize().Transform(To.SentenceCase),
                Size = c.Size.Humanize().Transform(To.TitleCase),
                StudentsCount = c.StudentsCount
            };
        }

        public static Company ConvertCreateDtoToEntity(CreateCompanyDto c)
        {
            return new Company
            {
                Name = c.Name,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber,
                Locality = c.Locality,
                ZipCode = c.ZipCode,
                Email = c.Email,
                AtivitySector = c.AtivitySector.DehumanizeTo<AtivitySectorEnum>(),
                Size = c.Size.DehumanizeTo<CompanySizeEnum>()
            };
        }

        public static Company ConvertUpdateDtoToEntity(UpdateCompanyDto c)
        {
            return new Company
            {
                Id = c.Id,
                Name = c.Name,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber,
                Locality = c.Locality,
                ZipCode = c.ZipCode,
                Email = c.Email,
                AtivitySector = c.AtivitySector.DehumanizeTo<AtivitySectorEnum>(),
                Size = c.Size.DehumanizeTo<CompanySizeEnum>()
            };
        }
    }
}
