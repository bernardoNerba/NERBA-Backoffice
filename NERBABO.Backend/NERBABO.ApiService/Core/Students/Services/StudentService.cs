using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Core.Students.Dtos;
using NERBABO.ApiService.Core.Students.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;
using ZLinq;

namespace NERBABO.ApiService.Core.Students.Services
{
    public class StudentService(
        ILogger<StudentService> logger,
        AppDbContext context
        ) : IStudentService
    {
        private readonly ILogger<StudentService> _logger = logger;
        private readonly AppDbContext _context = context;

        public async Task<Result<RetrieveStudentDto>> CreateStudentAsync(CreateStudentDto studentDto)
        {
            var person = await _context.People.FindAsync(studentDto.PersonId);
            var company = await _context.Companies.FindAsync(studentDto.CompanyId);

            // relationship validation
            if (person is null)
            {
                _logger.LogWarning("Person with id {id} not found", studentDto.PersonId);
                return Result<RetrieveStudentDto>
                    .Fail("Não encontrado", "Pessoa associada não encontrada.",
                    StatusCodes.Status404NotFound);
            }

            if (await _context.Students.AnyAsync(s => s.PersonId == studentDto.PersonId))
            {
                _logger.LogWarning("There is already a student associated with this person");
                return Result<RetrieveStudentDto>
                    .Fail("Erro de Validação", "Já existe um formando associado a esta pessoa.",
                    StatusCodes.Status404NotFound);
            }

            var student = Student.ConvertCreateDtoToEntity(studentDto, person, company);

            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            
            return Result<RetrieveStudentDto>
                .Ok( Student.ConvertEntityToRetrieveDto(student, company),"Formando Criado.", "Formando criado com sucesso.",
                StatusCodes.Status201Created);

        }

        public async Task<Result> DeleteStudentAsync(long id)
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

        public async Task<Result<RetrieveStudentDto>> GetStudentByIdAsync(long id)
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

        public async Task<Result<RetrieveStudentDto>> GetStudentByPersonIdAsync(long personId)
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

        public async Task<Result<RetrieveStudentDto>> UpdateStudentAsync(UpdateStudentDto studentDto)
        {
            var existingStudent = await _context.Students.FindAsync(studentDto.Id);
            var person = await _context.People.FindAsync(studentDto.PersonId);
            var company = await _context.Companies.FindAsync(studentDto.CompanyId);

            if (existingStudent is null)
                return Result<RetrieveStudentDto>
                    .Fail("Não encontrado", "Formando não encontrado.",
                    StatusCodes.Status404NotFound);

            // relationship validation
            if (person is null)
                return Result<RetrieveStudentDto>
                    .Fail("Não encontrado", "Pessoa associada não encontrada.",
                    StatusCodes.Status404NotFound);

            if (existingStudent.PersonId != studentDto.PersonId
                && await _context.Students.AnyAsync(s => s.PersonId == studentDto.PersonId))
            {
                _logger.LogWarning("There is already a student associated with this person");
                return Result<RetrieveStudentDto>
                    .Fail("Erro de Validação", "Já existe um formando associado a esta pessoa.",
                    StatusCodes.Status404NotFound);
            }

            var student = Student.ConvertCreateDtoToEntity(studentDto, person, company);

            _context.Entry(existingStudent).CurrentValues.SetValues(student);
            await _context.SaveChangesAsync();

            return Result<RetrieveStudentDto>
                .Ok(Student.ConvertEntityToRetrieveDto(student, company),
                "Formando Atualizado.", "Formando atualizado com sucesso.");
        }
    }
}
