using NERBABO.ApiService.Core.Modules.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Modules.Models
{
    public class Module : Entity
    {
        public string Name { get; set; } = string.Empty;
        public float Hours { get; set; }
        public bool IsActive { get; set; }

        public Module() {}

        public Module(long id ,string name, float hours, bool isActive)
        {
            Id = id;
            Name = name;
            Hours = hours;
            IsActive = isActive;
        }

        public Module(string name, float hours, bool isActive)
        {
            Name = name;
            Hours = hours;
            IsActive = isActive;
        }

        public static RetrieveModuleDto ConvertEntityToRetrieveDto(Module module)
        {
            return new RetrieveModuleDto
            {
                Id = module.Id,
                Name = module.Name,
                Hours = module.Hours,
                IsActive = module.IsActive
            };
        }

        public static Module ConvertCreateDtoToEntity(CreateModuleDto m)
        {
            return new Module(m.Name, m.Hours, true)
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Module ConvertUpdateDtoToEntity(UpdateModuleDto m)
        {
            return new Module(m.Id, m.Name, m.Hours, m.IsActive)
            {
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
