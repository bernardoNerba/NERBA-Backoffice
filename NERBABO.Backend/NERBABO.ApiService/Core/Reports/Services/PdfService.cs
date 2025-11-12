using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Core.Reports.Models;
using QuestPDF.Fluent;
using NERBABO.ApiService.Core.Reports.Composers;
using QuestPDF.Infrastructure;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Reports.Services;

public class PdfService : IPdfService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PdfService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly SessionsTimelineComposer _timelineComposer;
    private readonly CoverActionReportComposer _coverComposer;
    private readonly TeacherFormComposer _teacherFormComposer;
    private readonly CourseActionInformationReportComposer _courseActionInformationReportComposer;
    private readonly CourseActionProcessStudentPaymentsComposer _courseActionProcessStudentPaymentsComposer;
    private readonly string _pdfStoragePath;


    public PdfService(AppDbContext context, ILogger<PdfService> logger, IWebHostEnvironment environment,
        SessionsTimelineComposer timelineComposer, CoverActionReportComposer coverComposer, TeacherFormComposer teacherFormComposer,
        CourseActionInformationReportComposer courseActionInformationReportComposer, CourseActionProcessStudentPaymentsComposer courseActionProcessStudentPaymentsComposer)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
        _timelineComposer = timelineComposer;
        _coverComposer = coverComposer;
        _teacherFormComposer = teacherFormComposer;
        _courseActionInformationReportComposer = courseActionInformationReportComposer;
        _courseActionProcessStudentPaymentsComposer = courseActionProcessStudentPaymentsComposer;

        // Configure storage path
        _pdfStoragePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "storage", "pdfs");

        // Ensure storage directory exists
        Directory.CreateDirectory(_pdfStoragePath);

        // Configure QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<Result<byte[]>> GenerateSessionReportAsync(long actionId, string userId)
    {
        try
        {
            // Fetch action data
            var action = await _context.Actions
                .Include(a => a.Course).ThenInclude(c => c.Frame)
                .Include(a => a.Coordenator).ThenInclude(c => c.Person)
                .FirstOrDefaultAsync(a => a.Id == actionId);

            if (action is null)
            {
                _logger.LogWarning("Action not found for id {ActionId}", actionId);
                return Result<byte[]>
                    .Fail("Não encontrado.", "Ação não encontrada.", StatusCodes.Status404NotFound);
            }

            // Fetch sessions data
            var sessions = await _context.Sessions
                .Include(s => s.ModuleTeaching).ThenInclude(mt => mt.Module)
                .Include(s => s.ModuleTeaching).ThenInclude(mt => mt.Teacher).ThenInclude(t => t.Person)
                .Where(s => s.ModuleTeaching.ActionId == actionId)
                .OrderBy(s => s.ScheduledDate)
                .ToListAsync();

            // Fetch General Information
            var infos = await _context.GeneralInfo.FirstOrDefaultAsync()
                ?? throw new Exception("Failed to obtain general information.");

            var document = _timelineComposer.Compose(sessions, action, infos);
            var pdfBytes = document.GeneratePdf();

            // Save the new PDF
            var saveResult = await SavePdfAsync(PdfTypes.SessionReport, actionId, pdfBytes, userId);
            if (!saveResult.Success)
            {
                _logger.LogError("Failed to save PDF for action {ActionId}", actionId);
                return Result<byte[]>
                    .Fail("Erro interno.", "Falha ao guardar o relatório PDF.", StatusCodes.Status500InternalServerError);
            }

            _logger.LogInformation("Generated and saved new session report for action {ActionId}", actionId);
            return Result<byte[]>
                .Ok(pdfBytes, "Relatório criado.", $"Relatório de sessões criado com sucesso para a ação {actionId}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating session report for action {ActionId}", actionId);
            return Result<byte[]>
                .Fail("Erro interno.", "Ocorreu um erro ao gerar o relatório.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<byte[]>> GenerateCoverReportAsync(long actionId, string userId)
    {
        try
        {
            // Fetch action data with related entities
            var action = await _context.Actions
                .Include(a => a.Course).ThenInclude(c => c.Frame)
                .Include(a => a.Course).ThenInclude(c => c.Modules)
                .Include(a => a.Coordenator).ThenInclude(c => c.Person)
                .FirstOrDefaultAsync(a => a.Id == actionId);

            if (action is null)
            {
                _logger.LogWarning("Action not found for id {ActionId}", actionId);
                return Result<byte[]>
                    .Fail("Não encontrado.", "Ação não encontrada.", StatusCodes.Status404NotFound);
            }

            // Fetch General Information
            var infos = await _context.GeneralInfo.FirstOrDefaultAsync()
                ?? throw new Exception("Failed to obtain general information.");

            var document = _coverComposer.Compose(action, infos);
            var pdfBytes = document.GeneratePdf();

            // Save the new PDF
            var saveResult = await SavePdfAsync(PdfTypes.CoverActionReport, actionId, pdfBytes, userId);
            if (!saveResult.Success)
            {
                _logger.LogError("Failed to save cover PDF for action {ActionId}", actionId);
                return Result<byte[]>
                    .Fail("Erro interno.", "Falha ao guardar a capa PDF.", StatusCodes.Status500InternalServerError);
            }

            _logger.LogInformation("Generated and saved new cover report for action {ActionId}", actionId);
            return Result<byte[]>
                .Ok(pdfBytes, "Capa criada.", $"Capa do dossier criada com sucesso para a ação {actionId}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cover report for action {ActionId}", actionId);
            return Result<byte[]>
                .Fail("Erro interno.", "Ocorreu um erro ao gerar a capa.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<byte[]>> GenerateTeacherFormAsync(long actionId, long teacherId, string userId)
    {
        try
        {
            // Fetch action data with related entities
            var action = await _context.Actions
                .Include(a => a.Course).ThenInclude(c => c.Frame)
                .Include(a => a.Course).ThenInclude(c => c.Modules)
                .Include(a => a.Coordenator).ThenInclude(c => c.Person)
                .FirstOrDefaultAsync(a => a.Id == actionId);

            if (action is null)
            {
                _logger.LogWarning("Action not found for id {ActionId}", actionId);
                return Result<byte[]>
                    .Fail("Não encontrado.", "Ação não encontrada.", StatusCodes.Status404NotFound);
            }

            // Fetch teacher data with person information
            var teacher = await _context.Teachers
                .Include(t => t.Person)
                .Include(t => t.IvaRegime)
                .Include(t => t.IrsRegime)
                .FirstOrDefaultAsync(t => t.Id == teacherId);

            if (teacher is null)
            {
                _logger.LogWarning("Teacher not found for id {TeacherId}", teacherId);
                return Result<byte[]>
                    .Fail("Não encontrado.", "Formador não encontrado.", StatusCodes.Status404NotFound);
            }

            // Fetch module teachings for this teacher in this action
            var moduleTeachings = await _context.ModuleTeachings
                .Include(mt => mt.Module)
                .Include(mt => mt.Teacher).ThenInclude(t => t.Person)
                .Where(mt => mt.ActionId == actionId && mt.TeacherId == teacherId)
                .ToListAsync();

            if (!moduleTeachings.Any())
            {
                _logger.LogWarning("No module teachings found for teacher {TeacherId} in action {ActionId}", teacherId, actionId);
                return Result<byte[]>
                    .Fail("Não encontrado.", "Não foram encontrados módulos atribuídos a este formador nesta ação.", StatusCodes.Status404NotFound);
            }

            // Fetch General Information
            var infos = await _context.GeneralInfo.FirstOrDefaultAsync()
                ?? throw new Exception("Failed to obtain general information.");

            var document = _teacherFormComposer.Compose(teacher, action, moduleTeachings, infos);
            var pdfBytes = document.GeneratePdf();

            // Save the new PDF - use teacherId as reference since it's specific to this teacher
            var saveResult = await SavePdfAsync(PdfTypes.TeacherForm, teacherId, pdfBytes, userId);
            if (!saveResult.Success)
            {
                _logger.LogError("Failed to save teacher form PDF for teacher {TeacherId} in action {ActionId}", teacherId, actionId);
                return Result<byte[]>
                    .Fail("Erro interno.", "Falha ao guardar a ficha de formador PDF.", StatusCodes.Status500InternalServerError);
            }

            _logger.LogInformation("Generated and saved teacher form for teacher {TeacherId} in action {ActionId}", teacherId, actionId);
            return Result<byte[]>
                .Ok(pdfBytes, "Ficha criada.", $"Ficha de formador criada com sucesso para {teacher.Person.FullName}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating teacher form for teacher {TeacherId} in action {ActionId}", teacherId, actionId);
            return Result<byte[]>
                .Fail("Erro interno.", "Ocorreu um erro ao gerar a ficha de formador.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<byte[]>> GenerateTrainingFinancingFormAsync(long actionId, string userId)
    {
        try
        {
            // Fetch action data with related entities
            var action = await _context.Actions
                .Include(a => a.Course).ThenInclude(c => c.Frame)
                .Include(a => a.Course).ThenInclude(c => c.Modules)
                .Include(a => a.ModuleTeachings).ThenInclude(mt => mt.Sessions)
                .Include(a => a.Coordenator).ThenInclude(c => c.Person)
                .FirstOrDefaultAsync(a => a.Id == actionId);

            if (action is null)
            {
                _logger.LogWarning("Action not found for id {ActionId}", actionId);
                return Result<byte[]>
                    .Fail("Não encontrado.", "Ação não encontrada.", StatusCodes.Status404NotFound);
            }

            // Fetch General Information
            var infos = await _context.GeneralInfo.FirstOrDefaultAsync()
                ?? throw new Exception("Failed to obtain general information.");

            var document = _courseActionInformationReportComposer.Compose(action, infos);
            var pdfBytes = document.GeneratePdf();

            // Save the new PDF
            var saveResult = await SavePdfAsync(PdfTypes.CourseActionInformationReport, actionId, pdfBytes, userId);
            if (!saveResult.Success)
            {
                _logger.LogError("Failed to save course action information report PDF for action {ActionId}", actionId);
                return Result<byte[]>
                    .Fail("Erro interno.", "Falha ao guardar o relatório de informação da ação PDF.", StatusCodes.Status500InternalServerError);
            }

            _logger.LogInformation("Generated and saved course action information report for action {ActionId}", actionId);
            return Result<byte[]>
                .Ok(pdfBytes, "Relatório criado.", $"Relatório de informação da ação criado com sucesso para a ação {actionId}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating course action information report for action {ActionId}", actionId);
            return Result<byte[]>
                .Fail("Erro interno.", "Ocorreu um erro ao gerar o relatório de informação da ação.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<byte[]>> GenerateCourseActionProcessStudentPaymentsAsync(long actionId, string userId)
    {
        try
        {
            // Fetch action data with related entities
            var action = await _context.Actions
                .Include(a => a.Course).ThenInclude(c => c.Frame)
                .Include(a => a.Course).ThenInclude(c => c.Modules)
                .Include(a => a.ModuleTeachings).ThenInclude(mt => mt.Sessions)
                .Include(a => a.Coordenator).ThenInclude(c => c.Person)
                .FirstOrDefaultAsync(a => a.Id == actionId);

            if (action is null)
            {
                _logger.LogWarning("Action not found for id {ActionId}", actionId);
                return Result<byte[]>
                    .Fail("Não encontrado.", "Ação não encontrada.", StatusCodes.Status404NotFound);
            }

            var infos = await _context.GeneralInfo.FirstOrDefaultAsync()
                ?? throw new Exception("Failed to obtain general information.");
            
            var document = _courseActionProcessStudentPaymentsComposer.Compose(action, infos);
            var pdfBytes = document.GeneratePdf();

            // Save the new PDF
            var saveResult = await SavePdfAsync(PdfTypes.CourseActionProcessStudentPaymentsReport, actionId, pdfBytes, userId);
            if (!saveResult.Success)
            {
                _logger.LogError("Failed to save course action information report PDF for action {ActionId}", actionId);
                return Result<byte[]>
                    .Fail("Erro interno.", "Falha ao guardar o relatório de Processamento de Pagamentos dos Formandos da ação PDF.", StatusCodes.Status500InternalServerError);
            }

            _logger.LogInformation("Generated and saved course action information report for action {ActionId}", actionId);
            return Result<byte[]>
                .Ok(pdfBytes, "Relatório criado.", $"Relatório de Processamento de Pagamentos dos Formandos da ação criado com sucesso para a ação {actionId}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating course action process student payments report for action {ActionId}", actionId);
            return Result<byte[]>
                .Fail("Erro interno.", "Ocorreu um erro ao gerar o relatório de processamento de pagamentos dos formandos.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<SavedPdf>> GetSavedPdfAsync(string pdfType, long referenceId)
    {
        try
        {
            var savedPdf = await _context.SavedPdfs
                .FirstOrDefaultAsync(p => p.PdfType == pdfType && p.ReferenceId == referenceId);

            if (savedPdf is null)
            {
                return Result<SavedPdf>
                    .Fail("Não encontrado.", "PDF guardado não encontrado.", StatusCodes.Status404NotFound);
            }

            return Result<SavedPdf>.Ok(savedPdf);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting saved PDF {PdfType} for reference {ReferenceId}", pdfType, referenceId);
            return Result<SavedPdf>
                .Fail("Erro interno.", "Ocorreu um erro ao obter o PDF guardado.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<byte[]>> GetSavedPdfContentAsync(long savedPdfId)
    {
        try
        {
            // find the pdf
            var savedPdf = await _context.SavedPdfs.FindAsync(savedPdfId);

            if (savedPdf is null)
            {
                _logger.LogWarning("SavedPdf not found for id {Id}", savedPdfId);
                return Result<byte[]>
                    .Fail("Não encontrado.", "PDF guardado não encontrado.", StatusCodes.Status404NotFound);
            }

            if (!File.Exists(savedPdf.FilePath))
            {
                _logger.LogWarning("PDF file not found for SavedPdf {Id} at path {Path}", savedPdfId, savedPdf.FilePath);
                return Result<byte[]>
                    .Fail("Arquivo não encontrado.", "Arquivo PDF físico não encontrado.", StatusCodes.Status404NotFound);
            }

            var pdfContent = await File.ReadAllBytesAsync(savedPdf.FilePath);
            return Result<byte[]>.Ok(pdfContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading PDF file for SavedPdf {Id}", savedPdfId);
            return Result<byte[]>
                .Fail("Erro interno.", "Ocorreu um erro ao ler o arquivo PDF.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result> DeleteSavedPdfAsync(long savedPdfId)
    {
        try
        {
            var savedPdf = await _context.SavedPdfs.FindAsync(savedPdfId);

            if (savedPdf is null)
            {
                _logger.LogWarning("SavedPdf not found for id {Id}", savedPdfId);
                return Result
                    .Fail("Não encontrado.", "PDF guardado não encontrado.", StatusCodes.Status404NotFound);
            }

            // Delete physical file
            if (File.Exists(savedPdf.FilePath))
            {
                File.Delete(savedPdf.FilePath);
            }

            // Delete database record
            _context.SavedPdfs.Remove(savedPdf);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted saved PDF {Id} and its file", savedPdfId);
            return Result
                .Ok("PDF eliminado.", "PDF guardado eliminado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting saved PDF {Id}", savedPdfId);
            return Result
                .Fail("Erro interno.", "Ocorreu um erro ao eliminar o PDF.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<SavedPdf>> SavePdfAsync(string pdfType, long referenceId, byte[] pdfContent, string userId)
    {
        try
        {
            // hash the content to check for modifications in the new file
            var contentHash = SavedPdf.ComputeSha256Hash(pdfContent);

            // Check for existing PDF
            var existingPdfResult = await GetSavedPdfAsync(pdfType, referenceId);

            if (existingPdfResult.Success)
            {
                var existingPdf = existingPdfResult.Data!;
                if (existingPdf.ContentHash == contentHash)
                {
                    // Content is the same, no need to save
                    _logger.LogInformation("PDF content unchanged for {PdfType} {ReferenceId}", pdfType, referenceId);
                    return Result<SavedPdf>.Ok(existingPdf);
                }

                // There were modifications on the content so
                // Delete the old file
                if (File.Exists(existingPdf.FilePath))
                {
                    File.Delete(existingPdf.FilePath);
                }
                // Delete the Database entry
                _context.SavedPdfs.Remove(existingPdf);
            }

            // Generate filename and path
            var fileName = SavedPdf.GenerateFileName(pdfType, referenceId);
            var filePath = Path.Combine(_pdfStoragePath, fileName);

            // Save the file
            await File.WriteAllBytesAsync(filePath, pdfContent);

            // Create database record
            var savedPdf = new SavedPdf
            {
                PdfType = pdfType,
                ReferenceId = referenceId,
                FileName = fileName,
                FilePath = filePath,
                FileSizeBytes = pdfContent.Length,
                ContentHash = contentHash,
                GeneratedAt = DateTime.UtcNow,
                GeneratedByUserId = userId
            };

            _context.SavedPdfs.Add(savedPdf);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Saved PDF {PdfType} for reference {ReferenceId} as {FileName}",
                pdfType, referenceId, fileName);

            return Result<SavedPdf>
                .Ok(savedPdf, "PDF guardado.", $"PDF do tipo {pdfType} guardado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving PDF {PdfType} for reference {ReferenceId}", pdfType, referenceId);
            return Result<SavedPdf>
                .Fail("Erro interno.", "Ocorreu um erro ao guardar o PDF.", StatusCodes.Status500InternalServerError);
        }
    }

}