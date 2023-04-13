using Microsoft.AspNetCore.DataProtection;
using Umbraco.AuthorizedServices.Services.Implement;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class SecretEncryptorTests
{
    [Test]
    public void Encrypt_And_Decrypt()
    {
        // Arrange
        const string Message = "When seagulls follow the trawler, it is because they think sardines will be thrown into the sea.";
        var dataProtectionProvider = new EphemeralDataProtectionProvider();
        var sut = new DataProtectionSecretEncrytor(dataProtectionProvider);

        // Act
        var encryptedMessage = sut.Encrypt(Message);
        var decryptedMessage = sut.Decrypt(encryptedMessage);

        // Assert
        decryptedMessage.Should().Be(Message);
    }
}
