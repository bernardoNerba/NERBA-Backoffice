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

        public async Task<Result<RetrieveCompanyDto>> CreateCompanyAsync(CreateCompanyDto createCompanyDto)
        {
            
            if (await _context.Companies.AnyAsync(c => c.Name == createCompanyDto.Name))
            {
                _logger.LogWarning("Duplicated Company Name");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Nome da Empresa está duplicado.");
            }
            if (!string.IsNullOrEmpty(createCompanyDto.Email) 
                && await _context.Companies.AnyAsync(c => c.Email == createCompanyDto.Email))
            {
                _logger.LogWarning("Duplicated Comapny Email");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Email da Empresa está duplicado.");
            }
            if (!string.IsNullOrEmpty(createCompanyDto.ZipCode)
                && await _context.Companies.AnyAsync(c => c.ZipCode == createCompanyDto.ZipCode))
            {
                _logger.LogWarning("Duplicated Comapny ZipCode");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Código Postal da Empresa está duplicado.");
            }
            if (!string.IsNullOrEmpty(createCompanyDto.PhoneNumber)
                && await _context.Companies.AnyAsync(c => c.PhoneNumber == createCompanyDto.PhoneNumber))
            {
                _logger.LogWarning("Duplicated Comapny PhoneNumber");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Número de Telefone da Empresa está duplicado.");
            }

            //enums check
            if (!string.IsNullOrEmpty(createCompanyDto.AtivitySector)
                && !EnumHelp.IsValidEnum<AtivitySectorEnum>(createCompanyDto.AtivitySector))
            {
                return Result<RetrieveCompanyDto>
                    .Fail("Não encontrado", "Setor de atividade não encontrado.",
                    StatusCodes.Status404NotFound);
            }
            if(!string.IsNullOrEmpty(createCompanyDto.Size)
                && !EnumHelp.IsValidEnum<CompanySizeEnum>(createCompanyDto.Size))
            {
                return Result<RetrieveCompanyDto>
                    .Fail("Não encontrado", "Tamanho da Empresa não encontrado.",
                    StatusCodes.Status404NotFound);
            }

            var company = Company.ConvertCreateDtoToEntity(createCompanyDto);

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Company created successfully.");
            return Result<RetrieveCompanyDto>
                .Ok(Company.ConvertEntityToRetrieveDto(company));
        }

        public async Task<Result> DeleteCompanyAsync(long id)
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

        public async Task<Result<IEnumerable<RetrieveCompanyDto>>> GetAllCompaniesAsync()
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
                .Select(c => Company.ConvertEntityToRetrieveDto(c))
                .ToList();

            return Result<IEnumerable<RetrieveCompanyDto>>
                .Ok(orderedCompanies);
        }

        public async Task<Result<RetrieveCompanyDto>> GetCompanyAsync(long id)
        {
            var existingCompany = await _context.Companies.FindAsync(id);
            if (existingCompany is null)
                return Result<RetrieveCompanyDto>
                    .Fail("Não encontrado.", "Empresa não encontrada.",
                    StatusCodes.Status404NotFound);

            return Result<RetrieveCompanyDto>
                .Ok(Company.ConvertEntityToRetrieveDto(existingCompany));
        }

        public async Task<Result<RetrieveCompanyDto>> UpdateCompanyAsync(UpdateCompanyDto updateCompanyDto)
        {
            var existingCompany = await _context.Companies.FindAsync(updateCompanyDto.Id);
            if (existingCompany is null)
                return Result<RetrieveCompanyDto>
                    .Fail("Não encontrado.", "Empresa não encontrada", StatusCodes.Status404NotFound);

            if (existingCompany.Name != updateCompanyDto.Name
                && await _context.Companies.AnyAsync(c => c.Name == updateCompanyDto.Name))
            {
                _logger.LogWarning("Duplicated Company Name");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Nome da Empresa está duplicado.");
            }
            if (existingCompany.Email != updateCompanyDto.Email
                && !string.IsNullOrEmpty(updateCompanyDto.Email)
                && await _context.Companies.AnyAsync(c => c.Email == updateCompanyDto.Email))
            {
                _logger.LogWarning("Duplicated Comapny Email");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Email da Empresa está duplicado.");
            }
            if (existingCompany.ZipCode != updateCompanyDto.ZipCode
                && !string.IsNullOrEmpty(updateCompanyDto.ZipCode)
                && await _context.Companies.AnyAsync(c => c.ZipCode == updateCompanyDto.ZipCode))
            {
                _logger.LogWarning("Duplicated Comapny ZipCode");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Código Postal da Empresa está duplicado.");
            }
            if (existingCompany.PhoneNumber != updateCompanyDto.PhoneNumber
                && !string.IsNullOrEmpty(updateCompanyDto.PhoneNumber)
                && await _context.Companies.AnyAsync(c => c.PhoneNumber == updateCompanyDto.PhoneNumber))
            {
                _logger.LogWarning("Duplicated Comapny PhoneNumber");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Número de Telefone da Empresa está duplicado.");
            }


            //enums check
            if (!string.IsNullOrEmpty(updateCompanyDto.AtivitySector)
                && !EnumHelp.IsValidEnum<AtivitySectorEnum>(updateCompanyDto.AtivitySector))
            {
                return Result<RetrieveCompanyDto>
                    .Fail("Não encontrado", "Setor de atividade não encontrado.",
                    StatusCodes.Status404NotFound);
            }
            if (!string.IsNullOrEmpty(updateCompanyDto.Size)
                && !EnumHelp.IsValidEnum<CompanySizeEnum>(updateCompanyDto.Size))
            {
                return Result<RetrieveCompanyDto>
                    .Fail("Não encontrado", "Tamanho da Empresa não encontrado.",
                    StatusCodes.Status404NotFound);
            }

            var company = Company.ConvertUpdateDtoToEntity(updateCompanyDto);

            _context.Entry(existingCompany).CurrentValues.SetValues(company);
            _context.SaveChanges();

            _logger.LogInformation("Company updated successfully.");
            return Result<RetrieveCompanyDto>
                .Ok(Company.ConvertEntityToRetrieveDto(company),
                "Empresa Atualizada.", $"A empresa {company.Name} foi atualizada com sucesso.");
        }
    }
}
