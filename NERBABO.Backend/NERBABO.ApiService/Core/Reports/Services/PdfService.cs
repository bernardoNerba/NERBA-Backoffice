using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Core.Sessions.Models;
using NERBABO.ApiService.Core.Reports.Models;
using NERBABO.ApiService.Shared.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Security.Cryptography;
using System.Text;

namespace NERBABO.ApiService.Core.Reports.Services;

public class PdfService : IPdfService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PdfService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly string _pdfStoragePath;

    public PdfService(AppDbContext context, ILogger<PdfService> logger, IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
        
        // Configure storage path
        _pdfStoragePath = Path.Combine(_environment.ContentRootPath, "Storage", "PDFs");
        
        // Ensure storage directory exists
        Directory.CreateDirectory(_pdfStoragePath);
        
        // Configure QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateSessionReportAsync(long actionId, string userId)
    {
        _logger.LogInformation("Generating session report for action {ActionId} by user {UserId}", actionId, userId);

        // Check for existing PDF first
        var existingPdf = await GetSavedPdfAsync(PdfTypes.SessionReport, actionId);
        if (existingPdf != null)
        {
            var existingContent = await GetSavedPdfContentAsync(existingPdf.Id);
            if (existingContent != null)
            {
                _logger.LogInformation("Returning existing session report for action {ActionId}", actionId);
                return existingContent;
            }
        }

        var pdfBytes = await GenerateSessionReportPdfAsync(actionId);
        
        // Save the new PDF
        await SavePdfAsync(PdfTypes.SessionReport, actionId, pdfBytes, userId, true);
        
        _logger.LogInformation("Generated and saved new session report for action {ActionId}", actionId);
        return pdfBytes;
    }

    public async Task<byte[]> GenerateSessionDetailAsync(long sessionId, string userId)
    {
        _logger.LogInformation("Generating session detail for session {SessionId} by user {UserId}", sessionId, userId);

        var existingPdf = await GetSavedPdfAsync(PdfTypes.SessionDetail, sessionId);
        if (existingPdf != null)
        {
            var existingContent = await GetSavedPdfContentAsync(existingPdf.Id);
            if (existingContent != null)
            {
                _logger.LogInformation("Returning existing session detail for session {SessionId}", sessionId);
                return existingContent;
            }
        }

        var pdfBytes = await GenerateSessionDetailPdfAsync(sessionId);
        
        await SavePdfAsync(PdfTypes.SessionDetail, sessionId, pdfBytes, userId, true);
        
        _logger.LogInformation("Generated and saved new session detail for session {SessionId}", sessionId);
        return pdfBytes;
    }

    public async Task<byte[]> GenerateActionSummaryAsync(long actionId, string userId)
    {
        _logger.LogInformation("Generating action summary for action {ActionId} by user {UserId}", actionId, userId);

        var existingPdf = await GetSavedPdfAsync(PdfTypes.ActionSummary, actionId);
        if (existingPdf != null)
        {
            var existingContent = await GetSavedPdfContentAsync(existingPdf.Id);
            if (existingContent != null)
            {
                _logger.LogInformation("Returning existing action summary for action {ActionId}", actionId);
                return existingContent;
            }
        }

        // For now, action summary uses the same logic as session report
        var pdfBytes = await GenerateSessionReportPdfAsync(actionId);
        
        await SavePdfAsync(PdfTypes.ActionSummary, actionId, pdfBytes, userId, true);
        
        _logger.LogInformation("Generated and saved new action summary for action {ActionId}", actionId);
        return pdfBytes;
    }

    public async Task<SavedPdf?> GetSavedPdfAsync(string pdfType, long referenceId)
    {
        return await _context.SavedPdfs
            .FirstOrDefaultAsync(p => p.PdfType == pdfType && p.ReferenceId == referenceId);
    }

    public async Task<byte[]?> GetSavedPdfContentAsync(long savedPdfId)
    {
        var savedPdf = await _context.SavedPdfs.FindAsync(savedPdfId);
        if (savedPdf == null)
        {
            return null;
        }

        try
        {
            if (File.Exists(savedPdf.FilePath))
            {
                return await File.ReadAllBytesAsync(savedPdf.FilePath);
            }
            else
            {
                _logger.LogWarning("PDF file not found at path {FilePath} for SavedPdf {Id}", savedPdf.FilePath, savedPdfId);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading PDF file for SavedPdf {Id}", savedPdfId);
            return null;
        }
    }

    public async Task<bool> DeleteSavedPdfAsync(long savedPdfId)
    {
        var savedPdf = await _context.SavedPdfs.FindAsync(savedPdfId);
        if (savedPdf == null)
        {
            return false;
        }

        try
        {
            // Delete physical file
            if (File.Exists(savedPdf.FilePath))
            {
                File.Delete(savedPdf.FilePath);
            }

            // Delete database record
            _context.SavedPdfs.Remove(savedPdf);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted saved PDF {Id} and its file", savedPdfId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting saved PDF {Id}", savedPdfId);
            return false;
        }
    }

    public async Task<SavedPdf> SavePdfAsync(string pdfType, long referenceId, byte[] pdfContent, string userId, bool replaceExisting = false)
    {
        var contentHash = ComputeSha256Hash(pdfContent);
        
        // Check for existing PDF
        var existingPdf = await GetSavedPdfAsync(pdfType, referenceId);
        
        if (existingPdf != null)
        {
            if (existingPdf.ContentHash == contentHash)
            {
                // Content is the same, no need to save
                _logger.LogInformation("PDF content unchanged for {PdfType} {ReferenceId}", pdfType, referenceId);
                return existingPdf;
            }
            
            if (!replaceExisting)
            {
                throw new InvalidOperationException("PDF already exists and replaceExisting is false");
            }
            
            // Delete the old file
            if (File.Exists(existingPdf.FilePath))
            {
                File.Delete(existingPdf.FilePath);
            }
            
            _context.SavedPdfs.Remove(existingPdf);
        }

        // Generate filename and path
        var fileName = GenerateFileName(pdfType, referenceId);
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

        return savedPdf;
    }

    private async Task<byte[]> GenerateSessionReportPdfAsync(long actionId)
    {
        // Fetch action data
        var action = await _context.Actions
            .Include(a => a.Course)
            .Include(a => a.Coordenator).ThenInclude(c => c.Person)
            .FirstOrDefaultAsync(a => a.Id == actionId);

        if (action == null)
        {
            throw new ArgumentException("Ação não encontrada");
        }

        // Fetch sessions data
        var sessions = await _context.Sessions
            .Include(s => s.ModuleTeaching).ThenInclude(mt => mt.Module)
            .Include(s => s.ModuleTeaching).ThenInclude(mt => mt.Teacher).ThenInclude(t => t.Person)
            .Where(s => s.ModuleTeaching.ActionId == actionId)
            .OrderBy(s => s.ScheduledDate)
            .ToListAsync();

        // Generate PDF using existing logic
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                
                page.Header().Element(ComposeHeader);
                page.Content().Element(container => ComposeContent(container, action, sessions));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }
    
    private async Task<byte[]> GenerateSessionDetailPdfAsync(long sessionId)
    {
        var session = await _context.Sessions
            .Include(s => s.ModuleTeaching).ThenInclude(mt => mt.Module)
            .Include(s => s.ModuleTeaching).ThenInclude(mt => mt.Teacher).ThenInclude(t => t.Person)
            .Include(s => s.ModuleTeaching).ThenInclude(mt => mt.Action).ThenInclude(a => a.Course)
            .Include(s => s.ModuleTeaching).ThenInclude(mt => mt.Action).ThenInclude(a => a.Coordenator).ThenInclude(c => c.Person)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
        {
            throw new ArgumentException("Sessão não encontrada");
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header().Element(ComposeHeader);
                page.Content().Element(container => ComposeSessionDetail(container, session));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private string GenerateFileName(string pdfType, long referenceId)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        return $"{pdfType}_{referenceId}_{timestamp}.pdf";
    }

    private string ComputeSha256Hash(byte[] data)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(data);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    // Copy existing PDF composition methods from original PdfService
    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("NERBA - Sistema de Gestão")
                    .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                
                column.Item().Text("Relatório de Sessões")
                    .FontSize(16).SemiBold();
            });
            
            row.ConstantItem(100).AlignRight().Text($"Data: {DateTime.Now:dd/MM/yyyy}");
        });
    }

    private void ComposeContent(IContainer container, NERBABO.ApiService.Core.Actions.Models.CourseAction action, List<Session> sessions)
    {
        container.PaddingVertical(40).Column(column =>
        {
            column.Item().Element(container => ComposeActionInfo(container, action));
            column.Item().PaddingVertical(20);
            column.Item().Element(container => ComposeSessionsTable(container, sessions));
            column.Item().PaddingTop(20).Element(container => ComposeSummary(container, sessions));
        });
    }
    

    private void ComposeActionInfo(IContainer container, NERBABO.ApiService.Core.Actions.Models.CourseAction action)
    {
        container.Column(column =>
        {
            column.Item().Text("Informações da Ação").FontSize(14).SemiBold();
            
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Text($"Título: {action.Title}");
                row.RelativeItem().Text($"Coordenador: {action.Coordenator?.Person?.FullName ?? "N/A"}");
            });
            
            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Curso: {action.Course?.Title ?? "N/A"}");
                row.RelativeItem().Text($"Período: {action.StartDate:dd/MM/yyyy} - {action.EndDate:dd/MM/yyyy}");
            });
        });
    }

    private void ComposeSessionsTable(IContainer container, List<Session> sessions)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(60);
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.ConstantColumn(60);
                columns.ConstantColumn(50);
                columns.ConstantColumn(80);
            });

            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("Data").FontSize(10).SemiBold();
                header.Cell().Element(CellStyle).Text("Módulo").FontSize(10).SemiBold();
                header.Cell().Element(CellStyle).Text("Formador").FontSize(10).SemiBold();
                header.Cell().Element(CellStyle).Text("Horário").FontSize(10).SemiBold();
                header.Cell().Element(CellStyle).Text("Duração").FontSize(10).SemiBold();
                header.Cell().Element(CellStyle).Text("Presença").FontSize(10).SemiBold();
            });

            foreach (var session in sessions)
            {
                table.Cell().Element(CellStyle).Text(session.ScheduledDate.ToString("dd/MM")).FontSize(9);
                table.Cell().Element(CellStyle).Text(session.ModuleTeaching.Module?.Name ?? "N/A").FontSize(9);
                table.Cell().Element(CellStyle).Text(session.ModuleTeaching.Teacher?.Person?.FullName ?? "N/A").FontSize(9);
                table.Cell().Element(CellStyle).Text(GetTimeRange(session)).FontSize(9);
                table.Cell().Element(CellStyle).Text($"{session.DurationHours:F1}h").FontSize(9);
                table.Cell().Element(CellStyle).Text(session.TeacherPresence.ToString()).FontSize(9);
            }
        });

        static IContainer CellStyle(IContainer container) =>
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6);
    }

    private void ComposeSessionDetail(IContainer container, Session session)
    {
        container.PaddingVertical(40).Column(column =>
        {
            column.Item().Text("Detalhes da Sessão").FontSize(16).SemiBold();
            
            column.Item().PaddingTop(20).Column(details =>
            {
                details.Item().Text($"Módulo: {session.ModuleTeaching.Module?.Name ?? "N/A"}").FontSize(12);
                details.Item().Text($"Formador: {session.ModuleTeaching.Teacher?.Person?.FullName ?? "N/A"}").FontSize(12);
                details.Item().Text($"Data: {session.ScheduledDate:dd/MM/yyyy}").FontSize(12);
                details.Item().Text($"Horário: {GetTimeRange(session)}").FontSize(12);
                details.Item().Text($"Duração: {session.DurationHours:F1} horas").FontSize(12);
                details.Item().Text($"Dia da Semana: {session.Weekday}").FontSize(12);
                details.Item().Text($"Presença do Formador: {session.TeacherPresence}").FontSize(12);
            });

            column.Item().PaddingTop(30).Element(container => ComposeActionInfo(container, session.ModuleTeaching.Action));
        });
    }

    private void ComposeSummary(IContainer container, List<Session> sessions)
    {
        var totalHours = sessions.Sum(s => s.DurationHours);
        var completedSessions = sessions.Count(s => s.TeacherPresence.ToString() == "Present");
        
        container.Column(column =>
        {
            column.Item().Text("Resumo").FontSize(14).SemiBold();
            
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Text($"Total de Sessões: {sessions.Count}");
                row.RelativeItem().Text($"Sessões Realizadas: {completedSessions}");
                row.RelativeItem().Text($"Total de Horas: {totalHours:F1}h");
            });
        });
    }

    private string GetTimeRange(Session session)
    {
        var startTime = session.Start.ToString(@"HH\:mm");
        var endTime = session.Start.Add(TimeSpan.FromHours((double)session.DurationHours)).ToString(@"HH\:mm");
        return $"{startTime} - {endTime}";
    }
}