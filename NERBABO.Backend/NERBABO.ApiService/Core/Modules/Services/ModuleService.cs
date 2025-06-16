using NERBABO.ApiService.Core.Modules.Dtos;
using NERBABO.ApiService.Core.Modules.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Modules.Services
{
    public class ModuleService(
        AppDbContext context,
        ICacheService cacheService,
        ILogger<ModuleService> logger
        ) : IModuleService
    {
        private readonly AppDbContext _context = context;
        private readonly ICacheService _cacheService = cacheService;
        private readonly ILogger<ModuleService> _logger = logger;

        public Task<Result<Module>> CreateModuleAsync(CreateModuleDto moduleDto)
        {
            throw new NotImplementedException();
        }

        public Task<Result<bool>> DeleteModuleAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<Module>>> GetActiveModulesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<Module>>> GetAllModulesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<Module>> GetModuleByIdAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<Module>> UpdateModuleAsync(UpdateModuleDto moduleDto)
        {
            throw new NotImplementedException();
        }
    }
}
