using Xunit;
using BankingAPI.Services;
using System.Security.Cryptography;

namespace BankingAPI.Tests;

/// <summary>
/// Unit tests for EncryptionService
/// Section 6 - Available Encryption at Bank's end
/// </summary>
public class EncryptionServiceTests
{
    private readonly IEncryptionService _encryptionService;

    public EncryptionServiceTests()
    {
        _encryptionService = new EncryptionService();
    }

    #region RSA PKCS1 Encryption Tests

    [Fact]
    public void EncryptRsaPkcs1_ValidData_ReturnsEncryptedString()
    {
        // Arrange
        var plainText = "TestData123";
        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            var publicKeyXml = rsa.ToXmlString(false);

            // Act
            var encrypted = _encryptionService.EncryptRsaPkcs1(plainText, publicKeyXml, 2048);

            // Assert
            Assert.NotNull(encrypted);
            Assert.NotEmpty(encrypted);
            Assert.NotEqual(plainText, encrypted);
        }
    }

    [Fact]
    public void DecryptRsaPkcs1_ValidEncryptedData_ReturnsOriginalText()
    {
        // Arrange
        var plainText = "TestData123";
        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            var publicKeyXml = rsa.ToXmlString(false);
            var privateKeyXml = rsa.ToXmlString(true);

            var encrypted = _encryptionService.EncryptRsaPkcs1(plainText, publicKeyXml, 2048);

            // Act
            var decrypted = _encryptionService.DecryptRsaPkcs1(encrypted, privateKeyXml);

            // Assert
            Assert.Equal(plainText, decrypted);
        }
    }

    [Theory]
    [InlineData(1024)]
    [InlineData(2048)]
    [InlineData(4096)]
    public void EncryptRsaPkcs1_VariousKeySizes_SuccessfullyEncrypts(int keySize)
    {
        // Arrange
        var plainText = "TestMessage";
        using (var rsa = new RSACryptoServiceProvider(keySize))
        {
            var publicKeyXml = rsa.ToXmlString(false);

            // Act
            var encrypted = _encryptionService.EncryptRsaPkcs1(plainText, publicKeyXml, keySize);

            // Assert
            Assert.NotNull(encrypted);
            Assert.NotEmpty(encrypted);
        }
    }

    [Fact]
    public void DecryptRsaPkcs1_InvalidKey_ThrowsException()
    {
        // Arrange
        var privateKeyXml = "<invalid>key</invalid>";
        var encryptedData = "invalid_encrypted_data";

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _encryptionService.DecryptRsaPkcs1(encryptedData, privateKeyXml));
    }

    #endregion

    #region AES-CBC Encryption Tests

    [Fact]
    public void EncryptAesCbc_ValidData_ReturnsEncryptedString()
    {
        // Arrange
        var plainText = "TestData123";
        var key = new byte[16]; // 128-bit key
        var iv = new byte[16];  // 128-bit IV
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
            rng.GetBytes(iv);
        }

        // Act
        var encrypted = _encryptionService.EncryptAesCbc(plainText, key, iv);

        // Assert
        Assert.NotNull(encrypted);
        Assert.NotEmpty(encrypted);
        Assert.NotEqual(plainText, encrypted);
    }

    [Fact]
    public void DecryptAesCbc_ValidEncryptedData_ReturnsOriginalText()
    {
        // Arrange
        var plainText = "TestData123";
        var key = new byte[16];
        var iv = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
            rng.GetBytes(iv);
        }

        var encrypted = _encryptionService.EncryptAesCbc(plainText, key, iv);

        // Act
        var decrypted = _encryptionService.DecryptAesCbc(encrypted, key, iv);

        // Assert
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void EncryptAesCbc_DifferentIVs_ProducesDifferentCiphertexts()
    {
        // Arrange
        var plainText = "TestData123";
        var key = new byte[16];
        var iv1 = new byte[16];
        var iv2 = new byte[16];
        
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
            rng.GetBytes(iv1);
            rng.GetBytes(iv2);
        }

        // Act
        var encrypted1 = _encryptionService.EncryptAesCbc(plainText, key, iv1);
        var encrypted2 = _encryptionService.EncryptAesCbc(plainText, key, iv2);

        // Assert
        Assert.NotEqual(encrypted1, encrypted2);
    }

    [Fact]
    public void DecryptAesCbc_WrongKey_ThrowsException()
    {
        // Arrange
        var plainText = "TestData123";
        var key = new byte[16];
        var wrongKey = new byte[16];
        var iv = new byte[16];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
            rng.GetBytes(wrongKey);
            rng.GetBytes(iv);
        }

        var encrypted = _encryptionService.EncryptAesCbc(plainText, key, iv);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _encryptionService.DecryptAesCbc(encrypted, wrongKey, iv));
    }

    #endregion

    #region AES-ECB Encryption Tests

    [Fact]
    public void EncryptAesEcb_ValidData_ReturnsEncryptedString()
    {
        // Arrange
        var plainText = "TestData123";
        var key = new byte[24]; // 192-bit key for ECB
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }

        // Act
        var encrypted = _encryptionService.EncryptAesEcb(plainText, key);

        // Assert
        Assert.NotNull(encrypted);
        Assert.NotEmpty(encrypted);
        Assert.NotEqual(plainText, encrypted);
    }

    [Fact]
    public void DecryptAesEcb_ValidEncryptedData_ReturnsOriginalText()
    {
        // Arrange
        var plainText = "TestData123";
        var key = new byte[24];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }

        var encrypted = _encryptionService.EncryptAesEcb(plainText, key);

        // Act
        var decrypted = _encryptionService.DecryptAesEcb(encrypted, key);

        // Assert
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void EncryptAesEcb_SamePlaintext_ProducesSameCiphertext()
    {
        // Arrange
        var plainText = "TestData123";
        var key = new byte[24];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }

        // Act
        var encrypted1 = _encryptionService.EncryptAesEcb(plainText, key);
        var encrypted2 = _encryptionService.EncryptAesEcb(plainText, key);

        // Assert
        // ECB mode produces the same ciphertext for same plaintext (ECB weakness)
        Assert.Equal(encrypted1, encrypted2);
    }

    [Fact]
    public void DecryptAesEcb_WrongKey_ThrowsException()
    {
        // Arrange
        var plainText = "TestData123";
        var key = new byte[24];
        var wrongKey = new byte[24];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
            rng.GetBytes(wrongKey);
        }

        var encrypted = _encryptionService.EncryptAesEcb(plainText, key);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _encryptionService.DecryptAesEcb(encrypted, wrongKey));
    }

    #endregion

    #region Long Text Encryption Tests

    [Fact]
    public void EncryptAesCbc_LongText_SuccessfullyEncryptsAndDecrypts()
    {
        // Arrange
        var plainText = new string('A', 1000);
        var key = new byte[16];
        var iv = new byte[16];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
            rng.GetBytes(iv);
        }

        // Act
        var encrypted = _encryptionService.EncryptAesCbc(plainText, key, iv);
        var decrypted = _encryptionService.DecryptAesCbc(encrypted, key, iv);

        // Assert
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void EncryptAesEcb_LongText_SuccessfullyEncryptsAndDecrypts()
    {
        // Arrange
        var plainText = new string('A', 1000);
        var key = new byte[24];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }

        // Act
        var encrypted = _encryptionService.EncryptAesEcb(plainText, key);
        var decrypted = _encryptionService.DecryptAesEcb(encrypted, key);

        // Assert
        Assert.Equal(plainText, decrypted);
    }

    #endregion

    #region Special Characters and Edge Cases

    [Theory]
    [InlineData("123456789")]
    [InlineData("!@#$%^&*()")]
    [InlineData("Test\nWith\nNewlines")]
    [InlineData("Test\tWith\tTabs")]
    [InlineData("UTF8: 中文, العربية, Русский")]
    public void EncryptAesCbc_VariousCharacters_SuccessfullyEncryptsAndDecrypts(string plainText)
    {
        // Arrange
        var key = new byte[16];
        var iv = new byte[16];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
            rng.GetBytes(iv);
        }

        // Act
        var encrypted = _encryptionService.EncryptAesCbc(plainText, key, iv);
        var decrypted = _encryptionService.DecryptAesCbc(encrypted, key, iv);

        // Assert
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void EncryptAesCbc_EmptyString_SuccessfullyEncryptsAndDecrypts()
    {
        // Arrange
        var plainText = "";
        var key = new byte[16];
        var iv = new byte[16];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
            rng.GetBytes(iv);
        }

        // Act
        var encrypted = _encryptionService.EncryptAesCbc(plainText, key, iv);
        var decrypted = _encryptionService.DecryptAesCbc(encrypted, key, iv);

        // Assert
        Assert.Equal(plainText, decrypted);
    }

    #endregion

    #region Key Size Tests

    [Theory]
    [InlineData(16)]  // 128-bit
    [InlineData(24)]  // 192-bit
    [InlineData(32)]  // 256-bit
    public void EncryptAesCbc_VariousKeySizes_SuccessfullyEncryptsAndDecrypts(int keySizeBytes)
    {
        // Arrange
        var plainText = "TestData123";
        var key = new byte[keySizeBytes];
        var iv = new byte[16];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
            rng.GetBytes(iv);
        }

        // Act
        var encrypted = _encryptionService.EncryptAesCbc(plainText, key, iv);
        var decrypted = _encryptionService.DecryptAesCbc(encrypted, key, iv);

        // Assert
        Assert.Equal(plainText, decrypted);
    }

    #endregion
}
