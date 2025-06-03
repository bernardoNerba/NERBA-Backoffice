using Humanizer;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Core.Teachers.Dtos;
using NERBABO.ApiService.Core.Teachers.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Enums;

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
        var iva = await _context.Taxes.FindAsync(createTeacherDto.IvaRegimeId)
            ?? throw new KeyNotFoundException("Regime de IVA não encontrado.");

        var irs = await _context.Taxes.FindAsync(createTeacherDto.IrsRegimeId)
            ?? throw new KeyNotFoundException("Regime de IRS não encontrado.");

        var person = await _context.People.FindAsync(createTeacherDto.PersonId)
            ?? throw new KeyNotFoundException("Pessoa não encontrado.");

        if (iva.Type != TaxEnum.IVA)
        {
            _logger.LogError("Invalid tax types for Teacher creation.");
            throw new ArgumentException("IVA regime devem ser do tipo correto.");
        }

        if (irs.Type != TaxEnum.IRS)
        {
            _logger.LogError("Invalid tax types for Teacher creation.");
            throw new ArgumentException("IRS regime devem ser do tipo correto.");
        }

        if (await _context.Teachers.AnyAsync(t => t.PersonId == createTeacherDto.PersonId))
        {
            _logger.LogWarning("Teacher already exists for Person ID: {PersonId}", createTeacherDto.PersonId);
            throw new InvalidOperationException("Já existe um Formador associado a esta pessoa.");
        }

        if (await _context.Teachers.AnyAsync(t => t.Ccp == createTeacherDto.Ccp))
        {
            _logger.LogWarning("Teacher already exists with CCP: {Ccp}", createTeacherDto.Ccp);
            throw new InvalidOperationException("Já existe um Formador com este CCP.");
        }

        var teacher = Teacher.ConvertCreateDtoToTeacher(createTeacherDto, person, iva, irs);

        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Teacher created successfully with ID: {Id}", teacher.Id);
        return Teacher.ConvertTeacherToRetrieveDto(teacher);
    }

    public async Task<bool> DeleteTeacherAsync(long teacherId)
    {
        // TODO: Implemente deletion logic when ready
        var teacher = _context.Teachers.Find(teacherId)
            ?? throw new KeyNotFoundException("Formador não encontrado.");

        _context.Teachers.Remove(teacher);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<RetrieveTeacherDto> GetTeacherByPersonIdAsync(long personId)
    {
        var person = await _context.People.FindAsync(personId)
            ?? throw new KeyNotFoundException("Pessoa não encontrada");

        var teacher = await _context.Teachers
            .Include(t => t.IvaRegime)
            .Include(t => t.IrsRegime)
            .Include(t => t.Person)
            .FirstOrDefaultAsync(t => t.PersonId == personId)
            ?? throw new InvalidOperationException("Falha ao filtrar Formador.");

        return Teacher.ConvertTeacherToRetrieveDto(teacher);
    }

    public async Task<RetrieveTeacherDto> UpdateTeacherAsync(UpdateTeacherDto updateTeacherDto)
    {
        var existingTeacher = _context.Teachers.Find(updateTeacherDto.Id)
            ?? throw new KeyNotFoundException("Formador não encontrado.");

        var iva = await _context.Taxes.FindAsync(updateTeacherDto.IvaRegimeId)
            ?? throw new KeyNotFoundException("Regime de IVA não encontrado.");

        var irs = await _context.Taxes.FindAsync(updateTeacherDto.IrsRegimeId)
            ?? throw new KeyNotFoundException("Regime de IRS não encontrado.");

        var person = await _context.People.FindAsync(updateTeacherDto.PersonId)
            ?? throw new KeyNotFoundException("Pessoa não encontrado.");

        if (iva.Type != TaxEnum.IVA)
        {
            _logger.LogError("Invalid tax types for Teacher creation.");
            throw new ArgumentException("IVA regime devem ser do tipo correto.");
        }

        if (irs.Type != TaxEnum.IRS)
        {
            _logger.LogError("Invalid tax types for Teacher creation.");
            throw new ArgumentException("IRS regime devem ser do tipo correto.");
        }

        if (await _context.Teachers.AnyAsync(t => t.PersonId == updateTeacherDto.PersonId && t.Id != updateTeacherDto.Id))
        {
            _logger.LogWarning("Teacher already exists for Person ID: {PersonId}", updateTeacherDto.PersonId);
            throw new InvalidOperationException("Já existe um Formador associado a esta pessoa.");
        }

        if (await _context.Teachers.AnyAsync(t => t.Ccp == updateTeacherDto.Ccp && t.Id != updateTeacherDto.Id))
        {
            _logger.LogWarning("Teacher already exists with CCP: {Ccp}", updateTeacherDto.Ccp);
            throw new InvalidOperationException("Já existe um Formador com este CCP.");
        }

        var teacher = Teacher.ConvertUpdateDtoToTeacher(updateTeacherDto, person, iva, irs);


        _context.Entry(existingTeacher).CurrentValues.SetValues(teacher);
        _context.SaveChanges();

        _logger.LogInformation("Teacher updated successfully with ID: {Id}", existingTeacher.Id);
        return Teacher.ConvertTeacherToRetrieveDto(existingTeacher);
    }
}
