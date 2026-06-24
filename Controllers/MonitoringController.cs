using Microsoft.AspNetCore.Mvc;
using BankingAPI.Models;
using BankingAPI.Services;

namespace BankingAPI.Controllers;

/// <summary>
/// Health and Monitoring Controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MonitoringController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<MonitoringController> _logger;

    public MonitoringController(
        IAuditLogService auditLogService,
        ILogger<MonitoringController> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint
    /// GET /api/monitoring/health
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        try
        {
            var response = new HealthCheckResponse
            {
                Status = "Healthy",
                Version = "1.0.0",
                Details = new Dictionary<string, object>
                {
                    { "timestamp", DateTime.UtcNow },
                    { "environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production" },
                    { "dotnetVersion", Environment.Version.ToString() }
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new HealthCheckResponse
                {
                    Status = "Unhealthy",
                    Details = new Dictionary<string, object> { { "error", ex.Message } }
                });
        }
    }

    /// <summary>
    /// Get audit logs
    /// GET /api/monitoring/audit-logs?from=2024-01-01&to=2024-01-31&type=VALIDATION
    /// </summary>
    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(ApiResponse<List<AuditLog>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? type = null)
    {
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-1);
            var toDate = to ?? DateTime.UtcNow;

            if (fromDate > toDate)
            {
                return BadRequest(ApiResponse<List<AuditLog>>.ErrorResponse(
                    "From date must be before To date",
                    "INVALID_DATE_RANGE"));
            }

            var logs = await _auditLogService.GetAuditLogsAsync(fromDate, toDate, type);

            return Ok(ApiResponse<List<AuditLog>>.SuccessResponse(logs,
                $"Retrieved {logs.Count} audit logs"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<List<AuditLog>>.ErrorResponse(
                    "An error occurred while retrieving audit logs",
                    "INTERNAL_ERROR"));
        }
    }

    /// <summary>
    /// Get API statistics
    /// GET /api/monitoring/statistics?from=2024-01-01&to=2024-01-31
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<StatisticsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-1);
            var toDate = to ?? DateTime.UtcNow;

            var logs = await _auditLogService.GetAuditLogsAsync(fromDate, toDate);

            var stats = new StatisticsResponse
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalTransactionsProcessed = logs.Count,
                SuccessfulTransactions = logs.Count(l => l.Status == "SUCCESS"),
                FailedTransactions = logs.Count(l => l.Status == "FAILURE"),
                TotalAmountProcessed = logs.Sum(l => l.Amount),
                SuccessRate = logs.Count > 0 ? (double)logs.Count(l => l.Status == "SUCCESS") / logs.Count * 100 : 0,
                TransactionsByMode = logs
                    .Where(l => !string.IsNullOrEmpty(l.Details))
                    .GroupBy(l => ExtractPaymentMode(l.Details))
                    .ToDictionary(g => g.Key, g => g.Count()),
                TransactionsByStatus = logs
                    .GroupBy(l => l.Status)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return Ok(ApiResponse<StatisticsResponse>.SuccessResponse(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<StatisticsResponse>.ErrorResponse(
                    "An error occurred while retrieving statistics",
                    "INTERNAL_ERROR"));
        }
    }

    /// <summary>
    /// Clear old audit logs
    /// POST /api/monitoring/clear-audit-logs?retentionDays=90
    /// </summary>
    [HttpPost("clear-audit-logs")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearOldAuditLogs([FromQuery] int retentionDays = 90)
    {
        try
        {
            if (retentionDays < 1 || retentionDays > 365)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Retention days must be between 1 and 365",
                    "INVALID_RETENTION_DAYS"));
            }

            await _auditLogService.ClearOldAuditLogsAsync(retentionDays);

            return Ok(ApiResponse<object>.SuccessResponse(
                new { message = $"Cleared audit logs older than {retentionDays} days" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing audit logs");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    "An error occurred while clearing audit logs",
                    "INTERNAL_ERROR"));
        }
    }

    private static string ExtractPaymentMode(string? details)
    {
        if (string.IsNullOrEmpty(details)) return "Unknown";
        
        var modes = new[] { "NEFT", "RTGS", "IMPS", "UPI", "FT" };
        foreach (var mode in modes)
        {
            if (details.Contains(mode))
                return mode;
        }

        return "Unknown";
    }
}