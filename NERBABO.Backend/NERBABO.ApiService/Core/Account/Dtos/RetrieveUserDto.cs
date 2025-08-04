namespace NERBABO.ApiService.Core.Account.Dtos;

public class RetrieveUserDto
{
    public string Id { get; set; } = string.Empty;
    public long PersonId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public List<string> Roles { get; set; } = [];
    public bool IsActive { get; set; }
    public string LastLogin { get; set; } = string.Empty;
}
