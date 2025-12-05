using ClosedXML.Excel;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Shared.BulkImport.Services;

public class ExcelParserService(ILogger<ExcelParserService> logger
) : IFileParserService
{
    private readonly ILogger<ExcelParserService> _logger = logger;

    public async Task<Result<List<Dictionary<string, string>>>> ParseFileAsync(
        IFormFile file,
        FileType fileType)
    {
        if (fileType != FileType.Excel)
            return Result<List<Dictionary<string, string>>>
                .Fail("Tipo inválido", "Este serviço só processa ficheiros Excel.");

        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);  // First sheet

            var records = new List<Dictionary<string, string>>();

            // Get headers from first row
            var headerRow = worksheet.FirstRowUsed();
            if (headerRow == null)
                return Result<List<Dictionary<string, string>>>
                    .Fail("Ficheiro vazio", "O ficheiro Excel está vazio.");

            var headers = headerRow.Cells()
                .Select(c => c.GetString().Trim())
                .ToList();

            if (!headers.Any())
                return Result<List<Dictionary<string, string>>>
                    .Fail("Cabeçalhos em falta", "A primeira linha do Excel deve conter cabeçalhos.");

            // Read data rows
            var rows = worksheet.RowsUsed().Skip(1);  // Skip header
            foreach (var row in rows)
            {
                var rowData = new Dictionary<string, string>();
                for (int i = 0; i < headers.Count; i++)
                {
                    var cell = row.Cell(i + 1);
                    rowData[headers[i]] = cell.GetString().Trim();
                }
                records.Add(rowData);
            }

            return Result<List<Dictionary<string, string>>>.Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar ficheiro Excel");
            return Result<List<Dictionary<string, string>>>
                .Fail("Erro de Parsing", $"Erro ao ler ficheiro Excel: {ex.Message}");
        }
    }

    public Result ValidateFileStructure(
        IFormFile file,
        FileType fileType,
        List<string> requiredHeaders)
    {
        if (file == null || file.Length == 0)
            return Result.Fail("Ficheiro inválido", "Nenhum ficheiro foi fornecido.");

        var validExtensions = new[] { ".xlsx", ".xls" };
        if (!validExtensions.Any(ext => file.FileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            return Result.Fail("Extensão inválida", "O ficheiro deve ter extensão .xlsx ou .xls");

        try
        {
            using var stream = new MemoryStream();
            file.CopyTo(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            var headerRow = worksheet.FirstRowUsed();
            if (headerRow == null)
                return Result.Fail("Ficheiro vazio", "O ficheiro Excel está vazio.");

            var fileHeaders = headerRow.Cells()
                .Select(c => c.GetString().Trim())
                .ToList();

            var missingHeaders = requiredHeaders
                .Where(rh => !fileHeaders.Contains(rh, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (missingHeaders.Any())
                return Result.Fail(
                    "Cabeçalhos em falta",
                    $"Os seguintes cabeçalhos obrigatórios estão em falta: {string.Join(", ", missingHeaders)}");

            return Result.Ok("Válido", "Estrutura do ficheiro está válida.");
        }
        catch (Exception ex)
        {
            return Result.Fail("Erro de validação", $"Erro ao validar estrutura: {ex.Message}");
        }
    }
}
