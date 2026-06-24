using Microsoft.AspNetCore.Mvc;
using BankingAPI.Models;
using BankingAPI.Services;

namespace BankingAPI.Controllers;

/// <summary>
/// Credit Confirmation Controller - Handles credit confirmation requests
/// Section 3.3 & 3.4
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConfirmationController : ControllerBase
{
    private readonly ITransactionOrchestrationService _orchestrationService;
    private readonly ICreditConfirmationService _confirmationService;
    private readonly ILogger<ConfirmationController> _logger;

    public ConfirmationController(
        ITransactionOrchestrationService orchestrationService,
        ICreditConfirmationService confirmationService,
        ILogger<ConfirmationController> logger)
    {
        _orchestrationService = orchestrationService;
        _confirmationService = confirmationService;
        _logger = logger;
    }

    /// <summary>
    /// Process credit confirmation
    /// POST /api/confirmation/process
    /// </summary>
    [HttpPost("process")]
    [ProducesResponseType(typeof(ApiResponse<CreditConfirmationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreditConfirmationResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CreditConfirmationResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessConfirmation([FromBody] ApiRequest<CreditConfirmationRequest> request)
    {
        try
        {
            if (request?.Data == null)
            {
                return BadRequest(ApiResponse<CreditConfirmationResponse>.ErrorResponse(
                    "Request data is required",
                    "INVALID_REQUEST"));
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var response = await _orchestrationService.ProcessCreditConfirmationAsync(request.Data);
            sw.Stop();

            return Ok(new ApiResponse<CreditConfirmationResponse>
            {
                Success = response.Status == "A",
                Data = response,
                Message = response.Status == "A" ? "Confirmation processed successfully" : "Confirmation processing failed",
                DurationMs = (int)sw.ElapsedMilliseconds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing confirmation");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<CreditConfirmationResponse>.ErrorResponse(
                    "An error occurred while processing the request",
                    "INTERNAL_ERROR"));
        }
    }

    /// <summary>
    /// Get confirmation status
    /// GET /api/confirmation/status/{utr}
    /// </summary>
    [HttpGet("status/{utr}")]
    [ProducesResponseType(typeof(ApiResponse<CreditConfirmationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreditConfirmationResponse>), StatusCodes.Status404NotFound)]
    public IActionResult GetConfirmationStatus(string utr)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(utr))
            {
                return BadRequest(ApiResponse<CreditConfirmationResponse>.ErrorResponse(
                    "UTR is required",
                    "INVALID_REQUEST"));
            }

            var response = _confirmationService.GetConfirmationStatus(utr);

            if (response.Status == "R")
            {
                return NotFound(ApiResponse<CreditConfirmationResponse>.ErrorResponse(
                    response.Remarks ?? "Confirmation not found",
                    "NOT_FOUND"));
            }

            return Ok(ApiResponse<CreditConfirmationResponse>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving confirmation status for UTR: {UTR}", utr);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<CreditConfirmationResponse>.ErrorResponse(
                    "An error occurred while processing the request",
                    "INTERNAL_ERROR"));
        }
    }

    /// <summary>
    /// Process multiple confirmations (batch)
    /// POST /api/confirmation/process-batch
    /// </summary>
    [HttpPost("process-batch")]
    [ProducesResponseType(typeof(ApiResponse<List<CreditConfirmationResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessConfirmationBatch([FromBody] ApiRequest<List<CreditConfirmationRequest>> request)
    {
        try
        {
            if (request?.Data == null || request.Data.Count == 0)
            {
                return BadRequest(ApiResponse<List<CreditConfirmationResponse>>.ErrorResponse(
                    "Request data is required",
                    "INVALID_REQUEST"));
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var responses = new List<CreditConfirmationResponse>();

            foreach (var confirmationRequest in request.Data)
            {
                var response = await _orchestrationService.ProcessCreditConfirmationAsync(confirmationRequest);
                responses.Add(response);
            }

            sw.Stop();

            return Ok(new ApiResponse<List<CreditConfirmationResponse>>
            {
                Success = true,
                Data = responses,
                Message = $"Processed {responses.Count} confirmations",
                DurationMs = (int)sw.ElapsedMilliseconds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch confirmations");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<List<CreditConfirmationResponse>>.ErrorResponse(
                    "An error occurred while processing the request",
                    "INTERNAL_ERROR"));
        }
    }
}