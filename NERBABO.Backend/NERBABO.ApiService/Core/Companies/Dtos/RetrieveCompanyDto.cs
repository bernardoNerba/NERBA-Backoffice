
namespace NERBABO.ApiService.Core.Companies.Dtos
{
    public class RetrieveCompanyDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Locality { get; set; }
        public string? ZipCode { get; set; }
        public string? Email { get; set; }
        public string AtivitySector { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int StudentsCount { get; set; }

        public RetrieveCompanyDto() { }

        public RetrieveCompanyDto(long id, string name, string? address, string?
        phoneNumber, string? locality, string? zipCode, string? email, string ativitySector,
        string size, int studentsCount)
        {
            Id = id;
            Name = name;
            Address = address;
            PhoneNumber = phoneNumber;
            Locality = locality;
            ZipCode = zipCode;
            Email = email;
            AtivitySector = ativitySector;
            Size = size;
            StudentsCount = studentsCount;
        }
    }
}
