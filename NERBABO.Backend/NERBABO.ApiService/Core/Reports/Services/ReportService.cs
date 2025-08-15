using System;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Reports.Models;
using NERBABO.ApiService.Data;
using QuestPDF.Infrastructure;

namespace NERBABO.ApiService.Core.Reports.Services;

public class ReportService : IReportService
{
    private readonly ILogger<ReportService> _logger;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    private readonly string _pdfStoragePath;

    public ReportService(ILogger<ReportService> logger,
        AppDbContext context, IWebHostEnvironment environment) {
        _context = context;
        _logger = logger;
        _environment = environment;
        
        // Configure storage path
        _pdfStoragePath = Path.Combine(_environment.ContentRootPath, "Storage", "Reports");
        
        // Ensure storage directory exists
        Directory.CreateDirectory(_pdfStoragePath);
        
        // Configure QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
        }

    public async Task<Report?> GetSavedReportAsync(string reportType, long referenceId)
    {
        return await _context.Reports
            .FirstOrDefaultAsync(p => p.ReportType == reportType && p.ReferenceId == referenceId);
    }

    public async Task<bool> DeleteSavedPdfAsync(long savedPdfId)
    {
        var savedPdf = await _context.Reports.FindAsync(savedPdfId);
        if (savedPdf is null)
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
            _context.Reports.Remove(savedPdf);
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

    public async Task<Report> SavePdfAsync(string reportType, long referenceId, byte[] pdfContent, string userId, bool replaceExisting = false)
    {
        var contentHash = Report.ComputeSha256Hash(pdfContent);
        
        // Check for existing PDF
        var existingReport = await GetSavedReportAsync(reportType, referenceId);
        
        if (existingReport is not null)
        {
            if (existingReport.ContentHash == contentHash)
            {
                // Content is the same, no need to save
                _logger.LogInformation("PDF content unchanged for {PdfType} {ReferenceId}", existingReport, referenceId);
                return existingReport;
            }
            
            if (!replaceExisting)
            {
                throw new InvalidOperationException("PDF already exists and replaceExisting is false");
            }
            
            // Delete the old file
            if (File.Exists(existingReport.FilePath))
            {
                File.Delete(existingReport.FilePath);
            }
            
            _context.Reports.Remove(existingReport);
        }

        // Generate filename and path
        var fileName = Report.GenerateFileName(reportType, referenceId);
        var filePath = Path.Combine(_pdfStoragePath, fileName);

        // Save the file
        await File.WriteAllBytesAsync(filePath, pdfContent);

        // Create database record
        var savedReport = new Report
        {
            ReportType = reportType,
            ReferenceId = referenceId,
            FileName = fileName,
            FilePath = filePath,
            FileSizeBytes = pdfContent.Length,
            ContentHash = contentHash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            GeneratedByUserId = userId
        };

        _context.Reports.Add(savedReport);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Saved PDF {PdfType} for reference {ReferenceId} as {FileName}", 
            reportType, referenceId, fileName);

        return savedReport;
    }

}
