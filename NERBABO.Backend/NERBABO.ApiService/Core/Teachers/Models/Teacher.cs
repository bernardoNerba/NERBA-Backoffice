using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Core.ModuleTeachings.Models;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Core.Teachers.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Teachers.Models;

public class Teacher : Entity<long>
{
    public int IvaRegimeId { get; set; }
    public int IrsRegimeId { get; set; }
    public long PersonId { get; set; }

    /// <summary>
    /// Certificado de Competencias Pedagógicas
    /// </summary>
    public string Ccp { get; set; } = string.Empty;
    public string Competences { get; set; } = string.Empty;
    public float AvarageRating { get; set; } = 0.0f;
    public bool IsLecturingFM { get; set; }
    public bool IsLecturingCQ { get; set; }

    // Navigation properties
    public required Tax IvaRegime { get; set; }
    public required Tax IrsRegime { get; set; }
    public required Person Person { get; set; }
    public List<CourseAction> Action { get; set; } = [];
    public List<ModuleTeaching> ModuleTeachings { get; set; } = [];

    // Calculated properties
    public string CommaSeparatedCompetences => 
        string.Join(", ", Competences);


    public Teacher() { }

    public Teacher(long id, int ivaRegimeId, int irsRegimeId, long personId,
        string ccp, string competences, float avarageRating, bool isLecturingFM,
        bool isLecturingCQ)
    {
        Id = id;
        IvaRegimeId = ivaRegimeId;
        IrsRegimeId = irsRegimeId;
        PersonId = personId;
        Ccp = ccp;
        Competences = competences;
        AvarageRating = avarageRating;
        IsLecturingCQ = isLecturingCQ;
        IsLecturingFM = isLecturingFM;
    }

    public Teacher(int ivaRegimeId, int irsRegimeId, long personId, string ccp,
        string competences, float avarageRating, bool isLecturingFM,
        bool isLecturingCQ)
    {
        IvaRegimeId = ivaRegimeId;
        IrsRegimeId = irsRegimeId;
        PersonId = personId;
        Ccp = ccp;
        Competences = competences;
        AvarageRating = avarageRating;
        IsLecturingCQ = isLecturingCQ;
        IsLecturingFM = isLecturingFM;
    }

    public static Teacher ConvertCreateDtoToEntity(
        CreateTeacherDto createTeacherDto, Person person, Tax regimeIva, Tax regimeIrs)
    {
        return new Teacher(
            createTeacherDto.IvaRegimeId,
            createTeacherDto.IrsRegimeId,
            person.Id,
            createTeacherDto.Ccp,
            createTeacherDto.Competences,
            0.0f,
            false,
            false)
        {
            Person = person,
            IvaRegime = regimeIva,
            IrsRegime = regimeIrs,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public static RetrieveTeacherDto ConvertEntityToRetrieveDto(
        Teacher teacher)
    {
        return new RetrieveTeacherDto
        {
            Id = teacher.Id,
            PersonId = teacher.PersonId,
            IvaRegimeId = teacher.IvaRegimeId,
            IrsRegimeId = teacher.IrsRegimeId,
            IvaRegime = teacher.IvaRegime.Name,
            IrsRegime = teacher.IrsRegime.Name,
            Ccp = teacher.Ccp,
            Competences = teacher.Competences,
            AvarageRating = teacher.AvarageRating,
            IsLecturingCQ = teacher.IsLecturingCQ,
            IsLecturingFM = teacher.IsLecturingFM
        };
    }
}
