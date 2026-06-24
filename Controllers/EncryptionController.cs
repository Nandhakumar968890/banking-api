using Microsoft.AspNetCore.Mvc;
using BankingAPI.Models;
using BankingAPI.Services;

namespace BankingAPI.Controllers;

/// <summary>
/// Encryption/Decryption Controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EncryptionController : ControllerBase
{
    private readonly IEncryptionService _encryptionService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<EncryptionController> _logger;

    public EncryptionController(
        IEncryptionService encryptionService,
        IAuditLogService auditLogService,
        ILogger<EncryptionController> logger)
    {
        _encryptionService = encryptionService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <summary>
    /// Encrypt data using RSA
    /// POST /api/encryption/encrypt-rsa
    /// </summary>
    [HttpPost("encrypt-rsa")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EncryptRsa([FromBody] EncryptionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request?.PlainText) || string.IsNullOrEmpty(request?.PublicKey))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "PlainText and PublicKey are required",
                    "INVALID_REQUEST"));
            }

            var encrypted = _encryptionService.EncryptRsaPkcs1(request.PlainText, request.PublicKey, request.KeySize);
            await _auditLogService.LogEncryptionOperationAsync("RSA_ENCRYPT", "RSA/ECB/PKCS1Padding", true);

            return Ok(ApiResponse<object>.SuccessResponse(
                new { encryptedData = encrypted },
                "Data encrypted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting data with RSA");
            await _auditLogService.LogEncryptionOperationAsync("RSA_ENCRYPT", "RSA/ECB/PKCS1Padding", false, ex.Message);

            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    "An error occurred while encrypting data",
                    "ENCRYPTION_ERROR"));
        }
    }

    /// <summary>
    /// Decrypt data using RSA
    /// POST /api/encryption/decrypt-rsa
    /// </summary>
    [HttpPost("decrypt-rsa")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DecryptRsa([FromBody] DecryptionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request?.EncryptedData) || string.IsNullOrEmpty(request?.PrivateKey))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "EncryptedData and PrivateKey are required",
                    "INVALID_REQUEST"));
            }

            var decrypted = _encryptionService.DecryptRsaPkcs1(request.EncryptedData, request.PrivateKey);
            await _auditLogService.LogEncryptionOperationAsync("RSA_DECRYPT", "RSA/ECB/PKCS1Padding", true);

            return Ok(ApiResponse<object>.SuccessResponse(
                new { plainText = decrypted },
                "Data decrypted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting data with RSA");
            await _auditLogService.LogEncryptionOperationAsync("RSA_DECRYPT", "RSA/ECB/PKCS1Padding", false, ex.Message);

            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    "An error occurred while decrypting data",
                    "ENCRYPTION_ERROR"));
        }
    }

    /// <summary>
    /// Encrypt data using AES-CBC
    /// POST /api/encryption/encrypt-aes-cbc
    /// </summary>
    [HttpPost("encrypt-aes-cbc")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> EncryptAesCbc([FromBody] AesEncryptionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request?.PlainText) || 
                string.IsNullOrEmpty(request?.Key) || 
                string.IsNullOrEmpty(request?.IV))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "PlainText, Key, and IV are required",
                    "INVALID_REQUEST"));
            }

            var key = Convert.FromBase64String(request.Key);
            var iv = Convert.FromBase64String(request.IV);

            var encrypted = _encryptionService.EncryptAesCbc(request.PlainText, key, iv);
            await _auditLogService.LogEncryptionOperationAsync("AES_CBC_ENCRYPT", "AES/CBC/PKCS7Padding", true);

            return Ok(ApiResponse<object>.SuccessResponse(
                new { encryptedData = encrypted },
                "Data encrypted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting data with AES-CBC");
            await _auditLogService.LogEncryptionOperationAsync("AES_CBC_ENCRYPT", "AES/CBC/PKCS7Padding", false, ex.Message);

            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    "An error occurred while encrypting data",
                    "ENCRYPTION_ERROR"));
        }
    }

    /// <summary>
    /// Decrypt data using AES-CBC
    /// POST /api/encryption/decrypt-aes-cbc
    /// </summary>
    [HttpPost("decrypt-aes-cbc")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DecryptAesCbc([FromBody] AesDecryptionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request?.EncryptedData) || 
                string.IsNullOrEmpty(request?.Key) || 
                string.IsNullOrEmpty(request?.IV))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "EncryptedData, Key, and IV are required",
                    "INVALID_REQUEST"));
            }

            var key = Convert.FromBase64String(request.Key);
            var iv = Convert.FromBase64String(request.IV);

            var decrypted = _encryptionService.DecryptAesCbc(request.EncryptedData, key, iv);
            await _auditLogService.LogEncryptionOperationAsync("AES_CBC_DECRYPT", "AES/CBC/PKCS7Padding", true);

            return Ok(ApiResponse<object>.SuccessResponse(
                new { plainText = decrypted },
                "Data decrypted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting data with AES-CBC");
            await _auditLogService.LogEncryptionOperationAsync("AES_CBC_DECRYPT", "AES/CBC/PKCS7Padding", false, ex.Message);

            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    "An error occurred while decrypting data",
                    "ENCRYPTION_ERROR"));
        }
    }
}

/// <summary>
/// Request models for encryption endpoints
/// </summary>
public class EncryptionRequest
{
    public string? PlainText { get; set; }
    public string? PublicKey { get; set; }
    public int KeySize { get; set; } = 4096;
}

public class DecryptionRequest
{
    public string? EncryptedData { get; set; }
    public string? PrivateKey { get; set; }
}

public class AesEncryptionRequest
{
    public string? PlainText { get; set; }
    public string? Key { get; set; } // Base64 encoded
    public string? IV { get; set; } // Base64 encoded
}

public class AesDecryptionRequest
{
    public string? EncryptedData { get; set; }
    public string? Key { get; set; } // Base64 encoded
    public string? IV { get; set; } // Base64 encoded
}