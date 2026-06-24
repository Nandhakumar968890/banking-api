using Microsoft.AspNetCore.Mvc;
using BankingAPI.Models;
using BankingAPI.Services;

namespace BankingAPI.Controllers;

/// <summary>
/// Validation Controller - Handles transaction validation requests
/// Section 3.1 & 3.2
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ValidationController : ControllerBase
{
    private readonly ITransactionOrchestrationService _orchestrationService;
    private readonly ILogger<ValidationController> _logger;

    public ValidationController(
        ITransactionOrchestrationService orchestrationService,
        ILogger<ValidationController> logger)
    {
        _orchestrationService = orchestrationService;
        _logger = logger;
    }

    /// <summary>
    /// Validate a transaction
    /// POST /api/validation/validate
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ApiResponse<ValidationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ValidationResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateTransaction([FromBody] ApiRequest<ValidationRequest> request)
    {
        try
        {
            if (request?.Data == null)
            {
                return BadRequest(ApiResponse<ValidationResponse>.ErrorResponse(
                    "Request data is required",
                    "INVALID_REQUEST"));
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var response = await _orchestrationService.ProcessValidationRequestAsync(request.Data);
            sw.Stop();

            return Ok(new ApiResponse<ValidationResponse>
            {
                Success = response.Status == "A",
                Data = response,
                Message = response.Status == "A" ? "Transaction validated successfully" : "Transaction validation failed",
                DurationMs = (int)sw.ElapsedMilliseconds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating transaction");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<ValidationResponse>.ErrorResponse(
                    "An error occurred while processing the request",
                    "INTERNAL_ERROR"));
        }
    }

    /// <summary>
    /// Validate multiple transactions (batch)
    /// POST /api/validation/validate-batch
    /// </summary>
    [HttpPost("validate-batch")]
    [ProducesResponseType(typeof(ApiResponse<List<ValidationResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateTransactionBatch([FromBody] ApiRequest<List<ValidationRequest>> request)
    {
        try
        {
            if (request?.Data == null || request.Data.Count == 0)
            {
                return BadRequest(ApiResponse<List<ValidationResponse>>.ErrorResponse(
                    "Request data is required",
                    "INVALID_REQUEST"));
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var responses = new List<ValidationResponse>();

            foreach (var validationRequest in request.Data)
            {
                var response = await _orchestrationService.ProcessValidationRequestAsync(validationRequest);
                responses.Add(response);
            }

            sw.Stop();

            return Ok(new ApiResponse<List<ValidationResponse>>
            {
                Success = true,
                Data = responses,
                Message = $"Processed {responses.Count} transactions",
                DurationMs = (int)sw.ElapsedMilliseconds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating batch transactions");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<List<ValidationResponse>>.ErrorResponse(
                    "An error occurred while processing the request",
                    "INTERNAL_ERROR"));
        }
    }
}