using Humanizer;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Companies.Dtos;
using NERBABO.ApiService.Core.Companies.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Helper;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;
using System;
using ZLinq;

namespace NERBABO.ApiService.Core.Companies.Services
{
    public class CompanyService(
        AppDbContext context,
        ILogger<CompanyService> logger
    ) : ICompanyService
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<CompanyService> _logger = logger;


        public async Task<Result<RetrieveCompanyDto>> CreateAsync(CreateCompanyDto entityDto)
        {
            
            if (await _context.Companies.AnyAsync(c =>
                c.Name.ToLower()
                .Equals(entityDto.Name.ToLower())))
            {
                _logger.LogWarning("Duplicated Company Name");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Nome da Empresa está duplicado.");
            }
            if (!string.IsNullOrEmpty(entityDto.Email) 
                && await _context.Companies.AnyAsync(c =>
                (c.Email ?? "").ToLower().Equals(entityDto.Email.ToLower())))
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
                .Ok(Company.ConvertEntityToRetrieveDto(company),
                "Empresa criada.", "Empresa criada com sucesso.",
                StatusCodes.Status201Created);
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
            var existingCompanies = await _context.Companies
                .Include(c => c.Students)
                .ToListAsync();

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
            var existingCompany = await _context.Companies
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (existingCompany is null)
                return Result<RetrieveCompanyDto>
                    .Fail("Não encontrado.", "Empresa não encontrada.",
                    StatusCodes.Status404NotFound);

            return Result<RetrieveCompanyDto>
                .Ok(Company.ConvertEntityToRetrieveDto(existingCompany));
        }

        public async Task<Result<RetrieveCompanyDto>> UpdateAsync(UpdateCompanyDto entityDto)
        {
            var existingCompany = await _context.Companies
                .FindAsync(entityDto.Id);
            if (existingCompany is null)
                return Result<RetrieveCompanyDto>
                    .Fail("Não encontrado.", "Empresa não encontrada", StatusCodes.Status404NotFound);

            // check company name duplication
            if (await _context.Companies.AnyAsync(c =>
                c.Name.ToLower().Equals(entityDto.Name)
                && c.Id != entityDto.Id))
            {
                _logger.LogWarning("Duplicated Company Name");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Nome da Empresa está duplicado.");
            }

            // check company email duplication
            if (!string.IsNullOrEmpty(entityDto.Email)
                && await _context.Companies.AnyAsync(c =>
                (c.Email ?? "").ToLower().Equals(entityDto.Email)
                && c.Id != entityDto.Id)
                )
            {
                _logger.LogWarning("Duplicated Comapny Email");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Email da Empresa está duplicado.");
            }

            // check company zip code duplication
            if (!string.IsNullOrEmpty(entityDto.ZipCode)
                && await _context.Companies.AnyAsync(c =>
                c.ZipCode == entityDto.ZipCode
                && c.Id != entityDto.Id))
            {
                _logger.LogWarning("Duplicated Comapny ZipCode");
                return Result<RetrieveCompanyDto>
                    .Fail("Erro de Validação.", "Código Postal da Empresa está duplicado.");
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

            // Selective field updates - only update fields that have changed
            bool hasChanges = false;

            // Update Name if changed
            if (!string.Equals(existingCompany.Name, entityDto.Name, StringComparison.OrdinalIgnoreCase))
            {
                existingCompany.Name = entityDto.Name;
                hasChanges = true;
            }

            // Update Address if changed
            if (!string.Equals(existingCompany.Address, entityDto.Address))
            {
                existingCompany.Address = entityDto.Address;
                hasChanges = true;
            }

            // Update PhoneNumber if changed
            if (!string.Equals(existingCompany.PhoneNumber, entityDto.PhoneNumber))
            {
                existingCompany.PhoneNumber = entityDto.PhoneNumber;
                hasChanges = true;
            }

            // Update Locality if changed
            if (!string.Equals(existingCompany.Locality, entityDto.Locality))
            {
                existingCompany.Locality = entityDto.Locality;
                hasChanges = true;
            }

            // Update ZipCode if changed
            if (!string.Equals(existingCompany.ZipCode, entityDto.ZipCode))
            {
                existingCompany.ZipCode = entityDto.ZipCode;
                hasChanges = true;
            }

            // Update Email if changed
            if (!string.Equals(existingCompany.Email, entityDto.Email))
            {
                existingCompany.Email = entityDto.Email;
                hasChanges = true;
            }

            // Update AtivitySector if changed
            var newAtivitySector = entityDto.AtivitySector.DehumanizeTo<AtivitySectorEnum>();
            if (existingCompany.AtivitySector != newAtivitySector)
            {
                existingCompany.AtivitySector = newAtivitySector;
                hasChanges = true;
            }

            // Update Size if changed
            var newSize = entityDto.Size.DehumanizeTo<CompanySizeEnum>();
            if (existingCompany.Size != newSize)
            {
                existingCompany.Size = newSize;
                hasChanges = true;
            }

            // Return fail result if no changes were detected
            if (!hasChanges)
            {
                _logger.LogInformation("No changes detected for Company with ID {id}. No update performed.", entityDto.Id);
                return Result<RetrieveCompanyDto>
                    .Fail("Nenhuma alteração detetada.", "Não foi alterado nenhum dado. Modifique os dados e tente novamente.",
                    StatusCodes.Status400BadRequest);
            }

            // Update UpdatedAt and save changes
            existingCompany.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Company updated successfully.");
            return Result<RetrieveCompanyDto>
                .Ok(Company.ConvertEntityToRetrieveDto(existingCompany),
                "Empresa Atualizada.", $"A empresa {existingCompany.Name} foi atualizada com sucesso.");
        }
    }
}
