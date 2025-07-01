using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Students.Dtos;
using NERBABO.ApiService.Core.Students.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Students.Services
{
    public class StudentService(
        ILogger<StudentService> logger,
        AppDbContext context
        ) : IStudentService
    {
        private readonly ILogger<StudentService> _logger = logger;
        private readonly AppDbContext _context = context;

        public async Task<Result<RetrieveStudentDto>> CreateAsync(CreateStudentDto entityDto)
        {
            var person = await _context.People.FindAsync(entityDto.PersonId);
            var company = await _context.Companies.FindAsync(entityDto.CompanyId);

            // relationship validation
            if (person is null)
            {
                _logger.LogWarning("Person with id {id} not found", entityDto.PersonId);
                return Result<RetrieveStudentDto>
                    .Fail("Não encontrado", "Pessoa associada não encontrada.",
                    StatusCodes.Status404NotFound);
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
            
            return Result<RetrieveStudentDto>
                .Ok( Student.ConvertEntityToRetrieveDto(student, company),"Formando Criado.", "Formando criado com sucesso.",
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

            return Result
                .Ok("Formando eliminado.",
                "Formando eliminado com sucesso");

        }

        public async Task<Result<IEnumerable<RetrieveStudentDto>>> GetAllAsync()
        {
            var existingStudents = await _context.Students
                .Include(s => s.Company)
                .Select( s => Student.ConvertEntityToRetrieveDto(s, s.Company))
                .ToListAsync();

            if (existingStudents is null || existingStudents.Count == 0)
                return Result<IEnumerable<RetrieveStudentDto>>
                    .Fail("Não encontrado.", "Não foram encontrados resultados de formandos.",
                    StatusCodes.Status404NotFound);

            return Result<IEnumerable<RetrieveStudentDto>>
                .Ok(existingStudents);
        }

        public async Task<Result<IEnumerable<RetrieveStudentDto>>> GetByCompanyIdAsync(long companyId)
        {
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
                .Select(s => Student.ConvertEntityToRetrieveDto(s, s.Company))
                .ToListAsync();
            if (existingStudents is null || existingStudents.Count == 0)
            {
                _logger.LogWarning("No students found for company with id {id}", companyId);
                return Result<IEnumerable<RetrieveStudentDto>>
                    .Fail("Não encontrado.", "Não foram encontrados formandos para a empresa fornecida.",
                    StatusCodes.Status404NotFound);
            }

            return Result<IEnumerable<RetrieveStudentDto>>
                .Ok(existingStudents);
        }

        public async Task<Result<RetrieveStudentDto>> GetByIdAsync(long id)
        {
            var existingStudent = await _context.Students.FindAsync(id);

            if (existingStudent is null)
            {
                _logger.LogWarning("Student with id {id} not found", id);
                return Result<RetrieveStudentDto>
                    .Fail("Não encontrado.", "Formando não encontrado.",
                    StatusCodes.Status404NotFound);
            }
            
            var company = await _context.Companies.FindAsync(existingStudent.CompanyId);

            return Result<RetrieveStudentDto>
                .Ok(Student.ConvertEntityToRetrieveDto(existingStudent, company));
        }

        public async Task<Result<RetrieveStudentDto>> GetByPersonIdAsync(long personId)
        {
            var existingStudent = await _context.Students.FirstOrDefaultAsync(s => s.PersonId == personId);

            if (existingStudent is null)
                return Result<RetrieveStudentDto>
                    .Fail("Não encontrado.", "Não encontrado um formando com a pessoa fornecida.",
                    StatusCodes.Status404NotFound);

            var company = await _context.Companies.FindAsync(existingStudent.CompanyId);

            return Result<RetrieveStudentDto>
                .Ok(Student.ConvertEntityToRetrieveDto(existingStudent, company));
        }

        public async Task<Result<RetrieveStudentDto>> UpdateAsync(UpdateStudentDto entityDto)
        {
            var existingStudent = await _context.Students.FindAsync(entityDto.Id);
            var person = await _context.People.FindAsync(entityDto.PersonId);
            var company = await _context.Companies.FindAsync(entityDto.CompanyId);

            if (existingStudent is null)
                return Result<RetrieveStudentDto>
                    .Fail("Não encontrado", "Formando não encontrado.",
                    StatusCodes.Status404NotFound);

            // relationship validation
            if (person is null)
                return Result<RetrieveStudentDto>
                    .Fail("Não encontrado", "Pessoa associada não encontrada.",
                    StatusCodes.Status404NotFound);

            if (existingStudent.PersonId != entityDto.PersonId
                && await _context.Students.AnyAsync(s => s.PersonId == entityDto.PersonId))
            {
                _logger.LogWarning("There is already a student associated with this person");
                return Result<RetrieveStudentDto>
                    .Fail("Erro de Validação", "Já existe um formando associado a esta pessoa.",
                    StatusCodes.Status404NotFound);
            }

            var student = Student.ConvertCreateDtoToEntity(entityDto, person, company);

            _context.Entry(existingStudent).CurrentValues.SetValues(student);
            await _context.SaveChangesAsync();

            return Result<RetrieveStudentDto>
                .Ok(Student.ConvertEntityToRetrieveDto(student, company),
                "Formando Atualizado.", "Formando atualizado com sucesso.");
        }
    }
}
