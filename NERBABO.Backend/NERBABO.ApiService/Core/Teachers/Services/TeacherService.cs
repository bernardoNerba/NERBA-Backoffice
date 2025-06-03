using System;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Teachers.Dtos;
using NERBABO.ApiService.Core.Teachers.Models;
using NERBABO.ApiService.Data;

namespace NERBABO.ApiService.Core.Teachers.Services;

public class TeacherService : ITeacherService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TeacherService> _logger;
    public TeacherService(
        AppDbContext context,
        ILogger<TeacherService> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<RetrieveTeacherDto> CreateTeacherAsync(CreateTeacherDto createTeacherDto)
    {
        var iva = await _context.Taxes.FindAsync(createTeacherDto.IvaRegimeId);
        var irs = await _context.Taxes.FindAsync(createTeacherDto.IrsRegimeId);
        var person = await _context.People.FindAsync(createTeacherDto.PersonId);

        if (iva == null || irs == null || person == null)
        {
            _logger.LogError("IVA, IRS regime or Person not found for Teacher creation.");
            throw new ArgumentException("IVA, IRS regime ou Pessoa não encontrados.");
        }

        var teacher = Teacher.ConvertCreateDtoToTeacher(createTeacherDto, person, iva, irs);
        _context.Teachers.Add(teacher);
        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Teacher created successfully with ID: {Id}", teacher.Id);
            return Teacher.ConvertTeacherToRetrieveDto(teacher);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Teacher.");
            throw new Exception("Erro ao criar Formador.", ex);
        }
    }

    public async Task<RetrieveTeacherDto> GetTeacherByPersonIdAsync(long personId)
    {
        var person = await _context.People.FindAsync(personId);
        if (person == null)
        {
            _logger.LogWarning("Person not found for ID: {PersonId}", personId);
            throw new KeyNotFoundException("Pessoa não encontrada para o ID fornecido.");
        }

        var teacher = await _context.Teachers
            .Include(t => t.IvaRegime)
            .Include(t => t.IrsRegime)
            .Include(t => t.Person)
            .FirstOrDefaultAsync(t => t.PersonId == personId);

        if (teacher == null)
        {
            _logger.LogWarning("Teacher not found for Person ID: {PersonId}", personId);
            throw new KeyNotFoundException("Formador não encontrado para o ID da pessoa fornecido.");
        }

        return Teacher.ConvertTeacherToRetrieveDto(teacher);
    }
}
