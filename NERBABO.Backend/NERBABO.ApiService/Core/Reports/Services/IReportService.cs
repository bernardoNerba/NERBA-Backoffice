using System;
using NERBABO.ApiService.Core.Reports.Models;

namespace NERBABO.ApiService.Core.Reports.Services;

public interface IReportService
{
    Task<Report?> GetSavedReportAsync(string pdfType, long referenceId);
}
