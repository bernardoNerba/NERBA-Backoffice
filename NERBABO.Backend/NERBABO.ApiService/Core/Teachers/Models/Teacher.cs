using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Core.Teachers.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Teachers.Models;

public class Teacher : Entity
{
    public int IvaRegimeId { get; set; }
    public int IrsRegimeId { get; set; }
    public long PersonId { get; set; }
    public string Ccp { get; set; } = string.Empty;
    public string Competences { get; set; } = string.Empty;
    public float AvarageRating { get; set; } = 0.0f;
    public bool IsActive { get; set; }



    public required Tax IvaRegime { get; set; }
    public required Tax IrsRegime { get; set; }
    public required Person Person { get; set; }
    public string CommaSeparatedCompetences => string.Join(", ", Competences);


    public Teacher() { }

    public Teacher(long id, int ivaRegimeId, int irsRegimeId, long personId, string ccp, string competences, float avarageRating, bool isActive)
    {
        Id = id;
        IvaRegimeId = ivaRegimeId;
        IrsRegimeId = irsRegimeId;
        PersonId = personId;
        Ccp = ccp;
        Competences = competences;
        AvarageRating = avarageRating;
        IsActive = isActive;
    }

    public Teacher(int ivaRegimeId, int irsRegimeId, long personId, string ccp, string competences, float avarageRating, bool isActive)
    {
        IvaRegimeId = ivaRegimeId;
        IrsRegimeId = irsRegimeId;
        PersonId = personId;
        Ccp = ccp;
        Competences = competences;
        AvarageRating = avarageRating;
        IsActive = isActive;
    }

    public static Teacher ConvertCreateDtoToTeacher(
        CreateTeacherDto createTeacherDto, Person person, Tax regimeIva, Tax regimeIrs)
    {
        return new Teacher(
            createTeacherDto.IvaRegimeId,
            createTeacherDto.IrsRegimeId,
            person.Id,
            createTeacherDto.Ccp,
            createTeacherDto.Competences,
            0.0f,
            true)
        {
            Person = person,
            IvaRegime = regimeIva,
            IrsRegime = regimeIrs
        };
    }

    public static Teacher ConvertUpdateDtoToTeacher(
        UpdateTeacherDto updateTeacherDto, Person person, Tax regimeIva, Tax regimeIrs)
    {
        return new Teacher(
            updateTeacherDto.Id,
            updateTeacherDto.IvaRegimeId
            , updateTeacherDto.IrsRegimeId
            , person.Id
            , updateTeacherDto.Ccp
            , updateTeacherDto.Competences
            , 0.0f
            , updateTeacherDto.IsActive)
        {
            Person = person,
            IvaRegime = regimeIva,
            IrsRegime = regimeIrs
        };
    }

    public static RetrieveTeacherDto ConvertTeacherToRetrieveDto(
        Teacher teacher)
    {
        return new RetrieveTeacherDto
        {
            Id = teacher.Id,
            IvaRegime = teacher.IvaRegime.Name,
            IrsRegime = teacher.IrsRegime.Name,
            Ccp = teacher.Ccp,
            Competences = teacher.Competences,
            AvarageRating = teacher.AvarageRating,
            IsActive = teacher.IsActive
        };
    }
}
