using System;

namespace NERBABO.ApiService.Core.Authentication.Dtos;

public class LoggedInUserDto
{
    public LoggedInUserDto(string firstName, string lastName, string jWT)
    {
        FirstName = firstName;
        LastName = lastName;
        JWT = jWT;
    }

    public LoggedInUserDto()
    {

    }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string JWT { get; set; } = string.Empty;

}
