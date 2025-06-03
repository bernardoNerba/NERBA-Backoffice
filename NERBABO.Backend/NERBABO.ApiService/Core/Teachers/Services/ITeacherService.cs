using System;
using NERBABO.ApiService.Core.Teachers.Dtos;

namespace NERBABO.ApiService.Core.Teachers.Services;

public interface ITeacherService
{
    Task<RetrieveTeacherDto> CreateTeacherAsync(CreateTeacherDto createTeacherDto);
    Task<RetrieveTeacherDto> GetTeacherByPersonIdAsync(long personId);

}
