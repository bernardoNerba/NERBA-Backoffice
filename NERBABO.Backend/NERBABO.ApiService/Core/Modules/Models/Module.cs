using NERBABO.ApiService.Core.Courses.Models;
using NERBABO.ApiService.Core.Modules.Dtos;
using NERBABO.ApiService.Core.ModuleTeachings.Models;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Modules.Models
{
    public class ModuleCategory : Entity<long>
    {
        public string Name { get; set; } = string.Empty;
        public string ShortenName { get; set; } = string.Empty;

        // Navigation Properties
        public List<Module> Modules { get; set; } = [];

        public ModuleCategory(string name, string shortenName)
        {
            Name = name;
            ShortenName = shortenName;
        }

        public RetrieveCategoryDto ConvertEntityToRetrieveDto(ModuleCategory c)
        {
            return new RetrieveCategoryDto
            {
                Name = c.Name,
                ShortenName = c.ShortenName
            };
        }

        public static ModuleCategory ConvertCreateDtoToEntity(CreateCategoryDto c)
        {
            return new ModuleCategory(c.Name, c.ShortenName);
        }
    }

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
        public List<ModuleTeaching> ModuleTeachings { get; set; } = [];
        public List<ModuleCategory> Categories { get; set; } = [];

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
        public string AllDifferentCategories => String.Join(", ", Categories);
        public static RetrieveModuleDto ConvertEntityToRetrieveDto(Module module)
        {
            return new RetrieveModuleDto
            {
                Id = module.Id,
                Name = module.Name,
                Hours = module.Hours,
                IsActive = module.IsActive,
                CoursesQnt = module.CoursesQnt,
                AllDifferentCategories = module.AllDifferentCategories
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

        public static RetrieveModuleTeacherDto ConvertEntityToRetrieveModuleTeacherDto(Module module, long actionId)
        {
            // Find the module teaching for this specific action
            var moduleTeaching = module.ModuleTeachings.FirstOrDefault(mt => mt.ActionId == actionId);
            
            return new RetrieveModuleTeacherDto
            {
                Id = module.Id,
                Name = module.Name,
                Hours = module.Hours,
                IsActive = module.IsActive,
                CoursesQnt = module.CoursesQnt,
                TeacherId = moduleTeaching?.TeacherId,
                PersonId = moduleTeaching?.Teacher?.Person.Id,
                TeacherName = moduleTeaching?.Teacher?.Person != null 
                    ? $"{moduleTeaching.Teacher.Person.FirstName} {moduleTeaching.Teacher.Person.LastName}"
                    : null
            };
        }
    }
}
