using Humanizer;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Payments.Dtos;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Payments.Services;

public class PaymentsService(
    ILogger<PaymentsService> logger,
    AppDbContext context
) : IPaymentsService
{
    private readonly ILogger<PaymentsService> _logger = logger;
    private readonly AppDbContext _context = context;

    public async Task<Result<IEnumerable<TeacherPaymentsDto>>> GetAllTeacherPaymentsByActionIdAsync(long actionId)
    {
        var generalInfo = await _context.GeneralInfo.FirstOrDefaultAsync();
        if (generalInfo is null)
        {
            _logger.LogWarning("There is no GeneralInfo instances to fetch Teacher Payment.");
            return Result<IEnumerable<TeacherPaymentsDto>>
                .Fail("Não encontrar", "Não existe informações gerais no sistema.",
                StatusCodes.Status404NotFound);
        }

        var existingTeacherPayments = await _context.ModuleTeachings
            .AsNoTracking()
            .Include(mt => mt.Module)
            .Include(mt => mt.Teacher).ThenInclude(t => t.Person)
            .Include(mt => mt.Sessions)
            .Where(mt => mt.ActionId == actionId)
            .Select(mt => new TeacherPaymentsDto
            {
                ModuleTeachingId = mt.Id,
                ModuleId = mt.ModuleId,
                ModuleName = mt.Module.Name,
                TeacherPersonId = mt.Teacher.PersonId,
                TeacherName = mt.Teacher.Person.FullName,
                PaymentTotal = mt.PaymentTotal,
                CalculatedTotal = mt.CalculatedTotal(generalInfo.HourValueTeacher),
                PaymentDate = mt.PaymentDate.GetValueOrDefault().ToString("dd/MM/yyyy"),
                PaymentProcessed = mt.PaymentProcessed
            })
            .ToListAsync();

        if (existingTeacherPayments is null || existingTeacherPayments.Count == 0)
        {
            return Result<IEnumerable<TeacherPaymentsDto>>
                .Fail("Não encontrado.", "Não foram encontrados pagamentos dos formadores desta ação",
                    StatusCodes.Status404NotFound);
        }

        return Result<IEnumerable<TeacherPaymentsDto>>
            .Ok(existingTeacherPayments);
    }

    public async Task<Result> UpdateTeacherPaymentsByIdAsync(UpdateTeacherPaymentsDto dto)
    {
        var existingModuleTeaching = await _context.ModuleTeachings
            .FirstOrDefaultAsync(mt => mt.Id == dto.ModuleTeachingId);
        if (existingModuleTeaching is null)
        {
            return Result.Fail("Não encontrado", "Associação ModuleTeaching não encontrado",
                StatusCodes.Status404NotFound);
        }

        // check if payment date passed is valid DateOnly
        if (!DateOnly.TryParse(dto.PaymentDate, out DateOnly date))
        {
            return Result.Fail("Erro de Validação", "Data de Pagamento fornecida é inválida.");
        }

        // udpate existing module teaching payment properties with the dto data
        existingModuleTeaching.PaymentTotal = dto.PaymentTotal;
        existingModuleTeaching.PaymentDate = date;
        await _context.SaveChangesAsync();

        return Result
            .Ok("Pagamentos Atualizados", "Foram atualizados os pagamentos dos Formadores.");
    }

    public async Task<Result<IEnumerable<StudentPaymentsDto>>> GetAllStudentPaymentsByActionIdAsync(long actionId)
    {
        var generalInfo = await _context.GeneralInfo.FirstOrDefaultAsync();
        if (generalInfo is null)
        {
            _logger.LogWarning("There is no GeneralInfo instances to fetch Student Payment.");
            return Result<IEnumerable<StudentPaymentsDto>>
                .Fail("Não encontrar", "Não existe informações gerais no sistema.",
                    StatusCodes.Status404NotFound);
        }

        var existingStudentPayments = await _context.ActionEnrollments
            .AsNoTracking()
            .Include(ae => ae.Action)
            .Include(ae => ae.Student).ThenInclude(s => s.Person)
            .Include(ae => ae.Participations)
            .Where(ae => ae.ActionId == actionId)
            .Select(ae => new StudentPaymentsDto
            {
                ActionEnrollmentId = ae.Id,
                ActionId = ae.ActionId,
                ActionTitle = ae.Action.Title,
                StudentPersonId = ae.Student.PersonId,
                StudentName = ae.Student.Person.FullName,
                PaymentTotal = ae.PaymentTotal,
                CalculatedTotal = ae.CalculatedTotal(generalInfo.HourValueAlimentation),
                PaymentDate = ae.PaymentDate.GetValueOrDefault().ToString("dd/MM/yyyy"),
                PaymentProcessed = ae.PaymentProcessed
            })
            .ToListAsync()
            ?? [];

        if (existingStudentPayments is null || existingStudentPayments.Count == 0)
        {
            return Result<IEnumerable<StudentPaymentsDto>>
                .Fail("Não encontrado.", "Não foram encontrados pagamentos dos formandos desta ação",
                    StatusCodes.Status404NotFound);
        }

        return Result<IEnumerable<StudentPaymentsDto>>
            .Ok(existingStudentPayments);
    }
}