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
    }
}
