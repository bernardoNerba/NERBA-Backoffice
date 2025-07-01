using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Companies.Dtos;
using NERBABO.ApiService.Core.Companies.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Helper;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;
using ZLinq;

namespace NERBABO.ApiService.Core.Companies.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CompanyService> _logger;

        public CompanyService(
            AppDbContext context,
            ILogger<CompanyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<RetrieveCompanyDto>> CreateAsync(CreateCompanyDto entityDto)
        {
            
            if (await _context.Companies.AnyAsync(c => c.Name == entityDto.Name))
            {
                _logger.LogWarning("Duplicated Company Name");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Nome da Empresa está duplicado.");
            }
            if (!string.IsNullOrEmpty(entityDto.Email) 
                && await _context.Companies.AnyAsync(c => c.Email == entityDto.Email))
            {
                _logger.LogWarning("Duplicated Comapny Email");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Email da Empresa está duplicado.");
            }
            if (!string.IsNullOrEmpty(entityDto.ZipCode)
                && await _context.Companies.AnyAsync(c => c.ZipCode == entityDto.ZipCode))
            {
                _logger.LogWarning("Duplicated Comapny ZipCode");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Código Postal da Empresa está duplicado.");
            }
            if (!string.IsNullOrEmpty(entityDto.PhoneNumber)
                && await _context.Companies.AnyAsync(c => c.PhoneNumber == entityDto.PhoneNumber))
            {
                _logger.LogWarning("Duplicated Comapny PhoneNumber");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Número de Telefone da Empresa está duplicado.");
            }

            //enums check
            if (!string.IsNullOrEmpty(entityDto.AtivitySector)
                && !EnumHelp.IsValidEnum<AtivitySectorEnum>(entityDto.AtivitySector))
            {
                return Result<RetrieveCompanyDto>
                    .Fail("Não encontrado", "Setor de atividade não encontrado.",
                    StatusCodes.Status404NotFound);
            }
            if(!string.IsNullOrEmpty(entityDto.Size)
                && !EnumHelp.IsValidEnum<CompanySizeEnum>(entityDto.Size))
            {
                return Result<RetrieveCompanyDto>
                    .Fail("Não encontrado", "Tamanho da Empresa não encontrado.",
                    StatusCodes.Status404NotFound);
            }

            var company = Company.ConvertCreateDtoToEntity(entityDto);

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Company created successfully.");
            return Result<RetrieveCompanyDto>
                .Ok(Company.ConvertEntityToRetrieveDto(company));
        }

        public async Task<Result> DeleteAsync(long id)
        {
            var existingCompany = await _context.Companies.FindAsync(id);
            
            if (existingCompany is null)
            {
                _logger.LogWarning("Company with id {id} tryed to delete but not found", id);
                return Result
                    .Fail("Não encontrado.", "Empresa não encontrada.",
                    StatusCodes.Status404NotFound);
            }

            _context.Remove(existingCompany);
            await _context.SaveChangesAsync();

            return Result
                .Ok("Empresa eliminada", "Empresa eliminada com sucesso.");
        }

        public async Task<Result<IEnumerable<RetrieveCompanyDto>>> GetAllAsync()
        {
            var existingCompanies = await _context.Companies.ToListAsync();

            if (existingCompanies is null || existingCompanies.Count == 0)
                return Result<IEnumerable<RetrieveCompanyDto>>
                    .Fail("Não encontrado.", "Não foram encontradas empresas.",
                    StatusCodes.Status404NotFound);

            var orderedCompanies = existingCompanies
                .AsValueEnumerable()
                .OrderBy(c => c.Name)
                .ThenByDescending(c => c.Size)
                .Select(Company.ConvertEntityToRetrieveDto)
                .ToList();

            return Result<IEnumerable<RetrieveCompanyDto>>
                .Ok(orderedCompanies);
        }

        public async Task<Result<RetrieveCompanyDto>> GetByIdAsync(long id)
        {
            var existingCompany = await _context.Companies.FindAsync(id);
            if (existingCompany is null)
                return Result<RetrieveCompanyDto>
                    .Fail("Não encontrado.", "Empresa não encontrada.",
                    StatusCodes.Status404NotFound);

            return Result<RetrieveCompanyDto>
                .Ok(Company.ConvertEntityToRetrieveDto(existingCompany));
        }

        public async Task<Result<RetrieveCompanyDto>> UpdateAsync(UpdateCompanyDto entityDto)
        {
            var existingCompany = await _context.Companies.FindAsync(entityDto.Id);
            if (existingCompany is null)
                return Result<RetrieveCompanyDto>
                    .Fail("Não encontrado.", "Empresa não encontrada", StatusCodes.Status404NotFound);

            if (existingCompany.Name != entityDto.Name
                && await _context.Companies.AnyAsync(c => c.Name == entityDto.Name))
            {
                _logger.LogWarning("Duplicated Company Name");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Nome da Empresa está duplicado.");
            }
            if (existingCompany.Email != entityDto.Email
                && !string.IsNullOrEmpty(entityDto.Email)
                && await _context.Companies.AnyAsync(c => c.Email == entityDto.Email))
            {
                _logger.LogWarning("Duplicated Comapny Email");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Email da Empresa está duplicado.");
            }
            if (existingCompany.ZipCode != entityDto.ZipCode
                && !string.IsNullOrEmpty(entityDto.ZipCode)
                && await _context.Companies.AnyAsync(c => c.ZipCode == entityDto.ZipCode))
            {
                _logger.LogWarning("Duplicated Comapny ZipCode");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Código Postal da Empresa está duplicado.");
            }
            if (existingCompany.PhoneNumber != entityDto.PhoneNumber
                && !string.IsNullOrEmpty(entityDto.PhoneNumber)
                && await _context.Companies.AnyAsync(c => c.PhoneNumber == entityDto.PhoneNumber))
            {
                _logger.LogWarning("Duplicated Comapny PhoneNumber");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Número de Telefone da Empresa está duplicado.");
            }


            //enums check
            if (!string.IsNullOrEmpty(entityDto.AtivitySector)
                && !EnumHelp.IsValidEnum<AtivitySectorEnum>(entityDto.AtivitySector))
            {
                return Result<RetrieveCompanyDto>
                    .Fail("Não encontrado", "Setor de atividade não encontrado.",
                    StatusCodes.Status404NotFound);
            }
            if (!string.IsNullOrEmpty(entityDto.Size)
                && !EnumHelp.IsValidEnum<CompanySizeEnum>(entityDto.Size))
            {
                return Result<RetrieveCompanyDto>
                    .Fail("Não encontrado", "Tamanho da Empresa não encontrado.",
                    StatusCodes.Status404NotFound);
            }

            var company = Company.ConvertUpdateDtoToEntity(entityDto);

            _context.Entry(existingCompany).CurrentValues.SetValues(company);
            _context.SaveChanges();

            _logger.LogInformation("Company updated successfully.");
            return Result<RetrieveCompanyDto>
                .Ok(Company.ConvertEntityToRetrieveDto(company),
                "Empresa Atualizada.", $"A empresa {company.Name} foi atualizada com sucesso.");
        }
    }
}
