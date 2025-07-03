
using NERBABO.ApiService.Core.People.Models;

namespace NERBABO.ApiService.Core.People.Dtos;

public class RelationshipPersonDto
{
    public bool IsTeacher { get; set; }
    public bool IsStudent { get; set; }
    public bool IsColaborator { get; set; }

    public RelationshipPersonDto(Person p)
    {
        if (p.Teacher is not null)
            IsTeacher = true;

        if (p.Student is not null)
            IsStudent = true;

        if (p.User is not null)
        {
            IsColaborator = true;
        }        
    }
}