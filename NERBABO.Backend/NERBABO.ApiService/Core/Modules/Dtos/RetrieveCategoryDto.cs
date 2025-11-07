namespace NERBABO.ApiService.Core.Modules.Dtos;

public struct RetrieveCategoryDto
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string ShortenName { get; set; }
}