using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Companies.Models;
using NERBABO.ApiService.Core.People.Cache;
using NERBABO.ApiService.Core.Students.Cache;
using NERBABO.ApiService.Core.Students.Dtos;
using NERBABO.ApiService.Core.Students.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Students.Services
{
    public class StudentService(
        ILogger<StudentService> logger,
        AppDbContext context,
        ICacheStudentsRepository cache,
        ICachePeopleRepository cachePeople
        ) : IStudentService
    {
        private readonly ILogger<StudentService> _logger = logger;
        private readonly AppDbContext _context = context;
        private readonly ICacheStudentsRepository _cache = cache;
        private readonly ICachePeopleRepository _cachePeople = cachePeople;

        public async Task<Result<RetrieveStudentDto>> CreateAsync(CreateStudentDto entityDto)
        {
            Company? company = null;

            // relationship validation
            var person = await _context.People.FindAsync(entityDto.PersonId);
            if (person is null)
            {
                _logger.LogWarning("Person with id {id} not found", entityDto.PersonId);
                return Result<RetrieveStudentDto>
                    .Fail("Não encontrado", "Pessoa associada não encontrada.",
                    StatusCodes.Status404NotFound);
            }

            if (entityDto.CompanyId is not null
                && entityDto.CompanyId != 0)
            {
                company = await _context.Companies.FindAsync(entityDto.CompanyId);
                if (company is null)
                {
                    _logger.LogWarning("Company with id {id} not found", entityDto.CompanyId);
                    return Result<RetrieveStudentDto>
                        .Fail("Não encontrado", "Companhia associada não encontrada.");
                }
            }

            if (await _context.Students.AnyAsync(s => s.PersonId == entityDto.PersonId))
            {
                _logger.LogWarning("There is already a student associated with this person");
                return Result<RetrieveStudentDto>
                    .Fail("Erro de Validação", "Já existe um formando associado a esta pessoa.",
                    StatusCodes.Status404NotFound);
            }

            var student = Student.ConvertCreateDtoToEntity(entityDto, person, company);

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var retrieveStudent = Student.ConvertEntityToRetrieveDto(student, person, company);

            // update cache
            await RemoveRelatedCache(retrieveStudent.Id, retrieveStudent.PersonId);
            await _cache.SetSingleStudentsCacheAsync(retrieveStudent);

            return Result<RetrieveStudentDto>
                .Ok(retrieveStudent,
                "Formando Criado.", "Formando criado com sucesso.",
                StatusCodes.Status201Created);
        }

        public async Task<Result> DeleteAsync(long id)
        {
            var existingStudent = await _context.Students.FindAsync(id);

            if (existingStudent is null)
                return Result
                    .Fail("Não encontrado.", "Formando não encontrado.",
                    StatusCodes.Status404NotFound);

            _context.Students.Remove(existingStudent);
            await _context.SaveChangesAsync();

            // update cache
            await RemoveRelatedCache(existingStudent.Id, existingStudent.PersonId);

            return Result
                .Ok("Formando eliminado.",
                "Formando eliminado com sucesso");

        }

        public async Task<Result<IEnumerable<RetrieveStudentDto>>> GetAllAsync()
        {
            // try get students from cache
            var cachedStudents = await _cache.GetCacheAllStudentsAsync();
            if (cachedStudents is not null && cachedStudents.ToList().Count != 0)
                return Result<IEnumerable<RetrieveStudentDto>>
                .Ok(cachedStudents);

            // not on cache get from the database
            var existingStudents = await _context.Students
                .Include(s => s.Company)
                .Include(s => s.Person)
                .Select(s => Student.ConvertEntityToRetrieveDto(s, s.Person, s.Company))
                .ToListAsync();

            if (existingStudents is null || existingStudents.Count == 0)
                return Result<IEnumerable<RetrieveStudentDto>>
                    .Fail("Não encontrado.", "Não foram encontrados resultados de formandos.",
                    StatusCodes.Status404NotFound);

            // update cache
            await _cache.SetAllStudentsCacheAsync(existingStudents);

            return Result<IEnumerable<RetrieveStudentDto>>
                .Ok(existingStudents);
        }

        public async Task<Result<IEnumerable<RetrieveStudentDto>>> GetByCompanyIdAsync(long companyId)
        {
            // try retrieve the students of the company from caceh
            var cachedStudents = await _cache.GetStudentsByCompanyCacheAsync(companyId);
            if (cachedStudents is not null)
                return Result<IEnumerable<RetrieveStudentDto>>
                .Ok(cachedStudents);

            var existingCompany = await _context.Companies.FindAsync(companyId);
            if (existingCompany is null)
            {
                _logger.LogWarning("Company with id {id} not found", companyId);
                return Result<IEnumerable<RetrieveStudentDto>>
                    .Fail("Não encontrado.", "Empresa não encontrada.",
                    StatusCodes.Status404NotFound);
            }

            var existingStudents = await _context.Students
                .Where(s => s.CompanyId == companyId)
                .Include(s => s.Company)
                .Include(s => s.Person)
                .Select(s => Student.ConvertEntityToRetrieveDto(s, s.Person, s.Company))
                .ToListAsync();
            if (existingStudents is null || existingStudents.Count == 0)
            {
                _logger.LogWarning("No students found for company with id {id}", companyId);
                return Result<IEnumerable<RetrieveStudentDto>>
                    .Fail("Não encontrado.", "Não foram encontrados formandos para a empresa fornecida.",
                    StatusCodes.Status404NotFound);
            }

            // Update cache
            await _cache.SetStudentsByCompanyCacheAsync(companyId, existingStudents);

            return Result<IEnumerable<RetrieveStudentDto>>
                .Ok(existingStudents);
        }

        public async Task<Result<RetrieveStudentDto>> GetByIdAsync(long id)
        {
            // try retrieve the student from the cache
            var cachedStudent = await _cache.GetSingleStudentsCacheAsync(id);
            if (cachedStudent is not null)
                return Result<RetrieveStudentDto>
                    .Ok(cachedStudent);

            var existingStudent = await _context.Students.FindAsync(id);
            if (existingStudent is null)
            {
                _logger.LogWarning("Student with id {id} not found", id);
                return Result<RetrieveStudentDto>
                    .Fail("Não encontrado.", "Formando não encontrado.",
                    StatusCodes.Status404NotFound);
            }

            var company = await _context.Companies.FindAsync(existingStudent.CompanyId);
            var person = await _context.People.FindAsync(existingStudent.PersonId);

            var retrieveStudent = Student.ConvertEntityToRetrieveDto(existingStudent, person!, company);

            // update cache
            await _cache.SetSingleStudentsCacheAsync(retrieveStudent);

            return Result<RetrieveStudentDto>
                .Ok(retrieveStudent);
        }

        public async Task<Result<RetrieveStudentDto>> GetByPersonIdAsync(long personId)
        {
            // Check if the person exists
            var existingPerson = await _context.People
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == personId);
            if (existingPerson is null)
                return Result<RetrieveStudentDto>
                    .Fail("Não encontraod.", "Pessoa associada não encontrada.",
                    StatusCodes.Status404NotFound);

            // Check if the person is a student
            if (existingPerson.Student is null)
                return Result<RetrieveStudentDto>
                    .Fail("Não encontrado.", "Não encontrado um formando com a pessoa fornecida.",
                    StatusCodes.Status404NotFound);

            // Check cache for student
            var cachedStudent = await _cache.GetSingleStudentsCacheAsync(existingPerson.Student.Id);
            if (cachedStudent is not null)
                return Result<RetrieveStudentDto>
                    .Ok(cachedStudent);

            var company = await _context.Companies.FindAsync(existingPerson.Student.CompanyId);

            var retrieveStudent = Student.ConvertEntityToRetrieveDto(existingPerson.Student, existingPerson, company);

            // Update cache
            await _cache.SetSingleStudentsCacheAsync(retrieveStudent);

            return Result<RetrieveStudentDto>
                .Ok(retrieveStudent);
        }

        public async Task<Result<RetrieveStudentDto>> UpdateAsync(UpdateStudentDto entityDto)
        {
            Company? company = null;

            var existingStudent = await _context.Students
                .FindAsync(entityDto.Id);
            if (existingStudent is null)
                return Result<RetrieveStudentDto>
                    .Fail("Não encontrado", "Formando não encontrado.",
                    StatusCodes.Status404NotFound);

            // relationship validation
            var person = await _context.People
                .FindAsync(entityDto.PersonId);
            if (person is null)
                return Result<RetrieveStudentDto>
                    .Fail("Não encontrado", "Pessoa associada não encontrada.",
                    StatusCodes.Status404NotFound);


            if (entityDto.CompanyId is not null
                && entityDto.CompanyId != 0)
            {
                company = await _context.Companies
                    .FindAsync(entityDto.CompanyId);
                if (company is null)
                {
                    _logger.LogWarning("Company with id {id} not found", entityDto.CompanyId);
                    return Result<RetrieveStudentDto>
                        .Fail("Não encontrado", "Companhia associada não encontrada.");
                }
            }

            if (await _context.Students.AnyAsync(s =>
                s.PersonId == entityDto.PersonId
                && s.Id != entityDto.Id))
            {
                _logger.LogWarning("There is already a student associated with this person");
                return Result<RetrieveStudentDto>
                    .Fail("Erro de Validação", "Já existe um formando associado a esta pessoa.",
                    StatusCodes.Status404NotFound);
            }

            var student = Student.ConvertCreateDtoToEntity(entityDto, person, company);

            _context.Entry(existingStudent).CurrentValues.SetValues(student);
            await _context.SaveChangesAsync();

            var retrieveStudent = Student.ConvertEntityToRetrieveDto(student, person, company);

            // Update cache
            await RemoveRelatedCache(retrieveStudent.Id, retrieveStudent.PersonId);
            await _cache.SetSingleStudentsCacheAsync(retrieveStudent);

            return Result<RetrieveStudentDto>
                .Ok(retrieveStudent,
                "Formando Atualizado.", "Formando atualizado com sucesso.");
        }

        private async Task RemoveRelatedCache(long? id = null, long? personId = null)
        {
            await _cache.RemoveStudentsCacheAsync(id);
            await _cachePeople.RemovePeopleCacheAsync(personId);
        }
    }
}
