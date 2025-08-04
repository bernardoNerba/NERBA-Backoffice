using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Courses.Models;
using NERBABO.ApiService.Core.Modules.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Modules.Models
{
    public class Module : Entity<long>
    {
        // Entity Properties

        public string Name { get; set; } = string.Empty;
        public float Hours { get; set; }
        public bool IsActive { get; set; }

        // Calculated Properties
        public int CoursesQnt => Courses.Count;

        // Navigation Properties
        public List<Course> Courses { get; set; } = [];
        public List<TeacherModuleAction> TeacherModuleActions { get; set; } = [];


        // Constructors
        public Module() { }

        public Module(long id, string name, float hours, bool isActive)
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

        // Convert Methods
        public static RetrieveModuleDto ConvertEntityToRetrieveDto(Module module)
        {
            return new RetrieveModuleDto
            {
                Id = module.Id,
                Name = module.Name,
                Hours = module.Hours,
                IsActive = module.IsActive,
                CoursesQnt = module.CoursesQnt
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
    }
}
