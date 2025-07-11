﻿using NERBABO.ApiService.Core.Companies.Models;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Core.Students.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Students.Models
{
    public class Student : Entity<long>
    {
        public Student()
        {
        }

        public Student(long personId, long? companyId, bool isEmployeed, bool isRegisteredWithJobCenter, string? companyRole)
        {
            PersonId = personId;
            CompanyId = companyId;
            IsEmployeed = isEmployeed;
            IsRegisteredWithJobCenter = isRegisteredWithJobCenter;
            CompanyRole = companyRole;
        }

        public Student(long id, long personId, long? companyId, bool isEmployeed, bool isRegisteredWithJobCenter, string? companyRole)
        {
            Id = id;
            PersonId = personId;
            CompanyId = companyId;
            IsEmployeed = isEmployeed;
            IsRegisteredWithJobCenter = isRegisteredWithJobCenter;
            CompanyRole = companyRole;
        }

        public long PersonId { get; set; }
        public long? CompanyId { get; set; }
        public bool IsEmployeed { get; set; }
        public bool IsRegisteredWithJobCenter { get; set; }
        public string? CompanyRole { get; set; }

        public bool EnrolledInFM { get; set; }
        public bool EnrolledInCQ { get; set; }

        public required Person Person { get; set; }
        public Company? Company { get; set; }


        public static RetrieveStudentDto ConvertEntityToRetrieveDto(Student s, Person p, Company? c)
        {
            return new RetrieveStudentDto(
                s.Id,
                s.PersonId,
                c?.Id,
                c?.Name ?? "N/A",
                s.CompanyRole ?? "N/A",
                s.IsEmployeed,
                s.IsRegisteredWithJobCenter,
                $"{p.FirstName} {p.LastName}"
                );
        }

        public static Student ConvertCreateDtoToEntity(CreateStudentDto s, Person p, Company? c)
        {
            return new Student(
                p.Id,
                c?.Id,
                s.IsEmployeed,
                s.IsRegisteredWithJobCenter,
                s.CompanyRole
                )
            {
                Person = p,
                Company = c,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Student ConvertCreateDtoToEntity(UpdateStudentDto s, Person p, Company? c)
        {
            return new Student(
                s.Id,
                p.Id,
                c?.Id,
                s.IsEmployeed,
                s.IsRegisteredWithJobCenter,
                s.CompanyRole
                )
            {
                Person = p,
                Company = c,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
