using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Core.Reports.Models;
using QuestPDF.Fluent;
using NERBABO.ApiService.Core.Reports.Composers;
using QuestPDF.Infrastructure;
using System.Reflection.Metadata.Ecma335;

namespace NERBABO.ApiService.Core.Reports.Services;

public class PdfService : IPdfService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PdfService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly SessionsTimelineComposer _timelineComposer;
    private readonly string _pdfStoragePath;
    

    public PdfService(AppDbContext context, ILogger<PdfService> logger, IWebHostEnvironment environment, SessionsTimelineComposer timelineComposer)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
        _timelineComposer = timelineComposer;

        // Configure storage path
        _pdfStoragePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "storage", "pdfs");

        // Ensure storage directory exists
        Directory.CreateDirectory(_pdfStoragePath);

        // Configure QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateSessionReportAsync(long actionId, string userId)
    {
        // Fetch action data
        var action = await _context.Actions
            .Include(a => a.Course).ThenInclude(c => c.Frame)
            .Include(a => a.Coordenator).ThenInclude(c => c.Person)
            .FirstOrDefaultAsync(a => a.Id == actionId)
            ?? throw new ArgumentException("Ação não encontrada");

        // Fetch sessions data
        var sessions = await _context.Sessions
            .Include(s => s.ModuleTeaching).ThenInclude(mt => mt.Module)
            .Include(s => s.ModuleTeaching).ThenInclude(mt => mt.Teacher).ThenInclude(t => t.Person)
            .Where(s => s.ModuleTeaching.ActionId == actionId)
            .OrderBy(s => s.ScheduledDate)
            .ToListAsync();

        var document = _timelineComposer.Compose(sessions, action);
        var pdfBytes = document.GeneratePdf();
        
        // Save the new PDF
        await SavePdfAsync(PdfTypes.SessionReport, actionId, pdfBytes, userId);
        
        _logger.LogInformation("Generated and saved new session report for action {ActionId}", actionId);
        return pdfBytes;
    }

    public async Task<SavedPdf?> GetSavedPdfAsync(string pdfType, long referenceId)
    {
        return await _context.SavedPdfs
            .FirstOrDefaultAsync(p => p.PdfType == pdfType && p.ReferenceId == referenceId);
    }

    public async Task<byte[]?> GetSavedPdfContentAsync(long savedPdfId)
    {
        // find the pdf
        var savedPdf = await _context.SavedPdfs.FindAsync(savedPdfId);

        try
        {
            // found the pdf in the db and there is a physical file corresponding
            // return the file content in byte[]
            if (savedPdf is not null && File.Exists(savedPdf.FilePath))
            {
                return await File.ReadAllBytesAsync(savedPdf.FilePath);
            }
            
            _logger.LogWarning("PDF file not found for SavedPdf {Id}", savedPdfId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading PDF file for SavedPdf {Id}", savedPdfId);
        }
        return null;
    }

    public async Task<bool> DeleteSavedPdfAsync(long savedPdfId)
    {
        var savedPdf = await _context.SavedPdfs.FindAsync(savedPdfId);

        try
        {
            if (savedPdf is not null)
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

            _logger.LogWarning("PDF file not found for SavedPdf {Id}", savedPdfId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting saved PDF {Id}", savedPdfId);
        }

        return false;
    }

    public async Task<SavedPdf> SavePdfAsync(string pdfType, long referenceId, byte[] pdfContent, string userId)
    {
        // hash the content to check for modifications in the nenw file
        var contentHash = SavedPdf.ComputeSha256Hash(pdfContent);
        
        // Check for existing PDF
        var existingPdf = await GetSavedPdfAsync(pdfType, referenceId);
        
        if (existingPdf is not null)
        {
            if (existingPdf.ContentHash == contentHash)
            {
                // Content is the same, no need to save
                _logger.LogInformation("PDF content unchanged for {PdfType} {ReferenceId}", pdfType, referenceId);
                return existingPdf;
            }
            
            // There where modifications on the content so
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

        return savedPdf;
    }

}