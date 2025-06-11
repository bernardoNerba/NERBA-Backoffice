using NERBABO.ApiService.Core.Students.Dtos;
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

        public Task<Result<RetrieveStudentDto>> CreateStudentAsync(CreateStudentDto studentDto)
        {
            throw new NotImplementedException();
        }

        public Task<Result> DeleteStudentAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<RetrieveStudentDto>>> GetAllStudentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<RetrieveStudentDto>> GetStudentByIdAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<RetrieveStudentDto>> UpdateStudentAsync(UpdateStudentDto studentDto)
        {
            throw new NotImplementedException();
        }
    }
}
