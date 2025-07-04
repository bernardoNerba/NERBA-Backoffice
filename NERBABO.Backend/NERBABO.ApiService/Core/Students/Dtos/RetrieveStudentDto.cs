using System.Text.Json.Serialization;

namespace NERBABO.ApiService.Core.Students.Dtos
{
    public class RetrieveStudentDto
    {
        public RetrieveStudentDto(long id, long personId, long? companyId, string companyName,
        string companyRole, bool isEmployeed, bool isRegisteredWithJobCenter, string studentFullName)
        {
            Id = id;
            PersonId = personId;
            CompanyId = companyId;
            CompanyName = companyName;
            CompanyRole = companyRole;
            IsEmployeed = isEmployeed;
            IsRegisteredWithJobCenter = isRegisteredWithJobCenter;
            StudentFullName = studentFullName;
        }

        public long Id { get; set; }
        public long PersonId { get; set; }
        public long? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyRole { get; set; }
        public string StudentFullName { get; set; }

        [JsonIgnore]
        public bool IsEmployeed { get; set; }

        [JsonIgnore]
        public bool IsRegisteredWithJobCenter { get; set; }

        public string JobCenter => IsRegisteredWithJobCenter 
            ? "Registado no centro de emprego." 
            : "Não está registado no centro de emprego.";
        public string Employeed => IsEmployeed
            ? "Empregado"
            : "Desempregado";


    }
}
