using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Companies.Dtos;
using NERBABO.ApiService.Core.Companies.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;

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

            var company = Company.ConvertCreateDtoToEntity(createCompanyDto);

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Company created successfully.");
            return Result<RetrieveCompanyDto>
                .Ok(Company.ConvertEntityToRetrieveDto(company));
        }

        public Task<Result> DeleteCompanyAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<RetrieveCompanyDto>>> GetAllCompaniesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Result<RetrieveCompanyDto>> GetCompanyAsync(long id)
        {
            var existingCompany = await _context.Companies.FindAsync(id);
            if (existingCompany is null)
                return Result<RetrieveCompanyDto>
                    .Fail("Não encontrado.", "Empresa não encontrado.",
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

            var company = Company.ConvertUpdateDtoToEntity(updateCompanyDto);

            _context.Entry(existingCompany).CurrentValues.SetValues(company);
            _context.SaveChanges();

            _logger.LogInformation("Company updated successfully.");
            return Result<RetrieveCompanyDto>
                .Ok(Company.ConvertEntityToRetrieveDto(company),
                "Empresa Atualizada.", "Foi atualizado a empresa com sucesso.");
        }
    }
}
