﻿using Humanizer;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Actions.Dtos;
using NERBABO.ApiService.Core.Courses.Models;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Actions.Models
{
    public class CourseAction : Entity<long>
    {
        public long CourseId { get; set; }
        public string CoordenatorId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string AdministrationCode { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public string Locality { get; set; } = string.Empty;
        public List<WeekDaysEnum> WeekDays { get; set; } = [];
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public StatusEnum Status { get; set; } = StatusEnum.NotStarted;
        public RegimentTypeEnum Regiment { get; set; } = RegimentTypeEnum.hybrid;

        // Calculated properties
        public bool AllPaymentsProcessed
            => TeacherModuleActions.All(a => a.PaymentProcessed);

        public bool AllModulesOfActionHaveTeacher
            => Course.Modules.All(m =>
                TeacherModuleActions.Any(tma => tma.ModuleId == m.Id));

        // Navigation properties
        public required Course Course { get; set; }
        public required User Coordenator { get; set; }
        //public List<Student> Students { get; set; } = [];
        public List<TeacherModuleAction> TeacherModuleActions { get; set; } = [];


        public static RetrieveCourseActionDto ConvertEntityToRetrieveDto(CourseAction ca, User u, Course c)
        {
            return new RetrieveCourseActionDto
            {
                Id = ca.Id,

                CourseId = c.Id,
                CourseTitle = c.Title,
                CourseArea = c.Area,
                CourseMinHabilitationLevel = c.MinHabilitationLevel.Humanize().Transform(To.TitleCase),
                CourseTotalDuration = c.TotalDuration,
                CourseDestinators = [.. c.Destinators.Select(x => x.Humanize().Transform(To.TitleCase))],
                CourseModules = c.FormattedModuleNames,

                CoordenatorId = u.Id,
                CoordenatorName = u.UserName ?? "",

                Title = ca.Title,
                AdministrationCode = ca.AdministrationCode,
                Address = ca.Address,
                Locality = ca.Locality,
                WeekDays = [.. ca.WeekDays.Select(x => x.Humanize().Transform(To.SentenceCase))],
                StartDate = ca.StartDate.ToString("yyyy-MM-dd"),
                EndDate = ca.EndDate.ToString("yyyy-MM-dd"),
                Status = ca.Status.Humanize().Transform(To.TitleCase),
                Regiment = ca.Regiment.Humanize().Transform(To.TitleCase)
            };
        }

        public static CourseAction ConvertCreateDtoToEntity(CreateCourseActionDto ca, User u, Course c)
        {
            return new CourseAction
            {
                CourseId = c.Id,
                CoordenatorId = u.Id,
                Title = ca.Title,
                AdministrationCode = ca.AdministrationCode,
                Address = ca.Address,
                Locality = ca.Locality,
                WeekDays = [.. ca.WeekDays.Select(x => x.DehumanizeTo<WeekDaysEnum>())],
                StartDate = Helper.StringDateOnlyConverter.ConvertToDateOnly(ca.StartDate) ?? new DateOnly(),
                EndDate = Helper.StringDateOnlyConverter.ConvertToDateOnly(ca.EndDate) ?? new DateOnly(),
                Status = ca.Status.DehumanizeTo<StatusEnum>(),
                Regiment = ca.Regiment.DehumanizeTo<RegimentTypeEnum>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Coordenator = u,
                Course = c
            };
        }

        public static CourseAction ConvertUpdateDtoToEntity(UpdateCourseActionDto ca, User u, Course c)
        {
            return new CourseAction
            {
                Id = ca.Id,
                CourseId = c.Id,
                CoordenatorId = u.Id,
                Title = ca.Title,
                AdministrationCode = ca.AdministrationCode,
                Address = ca.Address,
                Locality = ca.Locality,
                WeekDays = [.. ca.WeekDays.Select(x => x.DehumanizeTo<WeekDaysEnum>())],
                StartDate = Helper.StringDateOnlyConverter.ConvertToDateOnly(ca.StartDate) ?? new DateOnly(),
                EndDate = Helper.StringDateOnlyConverter.ConvertToDateOnly(ca.EndDate) ?? new DateOnly(),
                Status = ca.Status.DehumanizeTo<StatusEnum>(),
                Regiment = ca.Regiment.DehumanizeTo<RegimentTypeEnum>(),
                UpdatedAt = DateTime.UtcNow,
                Coordenator = u,
                Course = c
            };
        }
    
    
    }
}
