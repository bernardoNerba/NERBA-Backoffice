namespace NERBABO.ApiService.Shared.BulkImport.Models;

/// <summary>
/// Represents the result of a bulk import operation
/// </summary>
public class BulkImportResult<T>
{
    public bool Success { get; set; }
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public int SkippedCount { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public TimeSpan Duration => CompletedAt - StartedAt;

    public List<ImportRowResult<T>> Results { get; set; } = new();
    public List<string> GlobalErrors { get; set; } = new();  // File-level errors

    public string Summary =>
        $"Importação concluída: {SuccessCount} com sucesso, {FailureCount} falhadas, {SkippedCount} ignoradas de {TotalRows} linhas.";
}

/// <summary>
/// Result for a single row import
/// </summary>
public class ImportRowResult<T>
{
    public int RowNumber { get; set; }
    public bool Success { get; set; }
    public T? Data { get; set; }  // Created entity (if successful)
    public List<ImportValidationError> Errors { get; set; } = new();
    public Dictionary<string, string> RowData { get; set; } = new();  // Original row data
}

/// <summary>
/// Detailed validation error for a row
/// </summary>
public class ImportValidationError
{
    public string Field { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? AttemptedValue { get; set; }
    public ErrorSeverity Severity { get; set; } = ErrorSeverity.Error;
}

public enum ErrorSeverity
{
    Warning,  // Non-blocking
    Error     // Blocking
}

/// <summary>
/// Real-time progress tracking (for large imports)
/// </summary>
public class ImportProgressInfo
{
    public Guid ImportId { get; set; }
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public ImportStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public string? CurrentPhase { get; set; }

    public decimal PercentComplete => TotalRows > 0
        ? Math.Round((decimal)ProcessedRows / TotalRows * 100, 2)
        : 0;
}

public enum ImportStatus
{
    Pending,
    Parsing,
    Validating,
    Importing,
    Completed,
    Failed,
    PartialSuccess
}
