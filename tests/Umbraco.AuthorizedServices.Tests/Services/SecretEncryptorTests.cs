using Umbraco.AuthorizedServices.Services.Implement;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class SecretEncryptorTests
{
    [Test]
    public void Encrypt_And_Decrypt()
    {
        // Arrange
        const string Key = "abcdef";
        const string Message = "When seagulls follow the trawler, it is because they think sardines will be thrown into the sea.";
        var sut = new SecretEncryptor(Key);

        // Act
        var encryptedMessage = sut.Encrypt(Message);
        var decryptedMessage = sut.Decrypt(encryptedMessage);

        // Assert
        decryptedMessage.Should().Be(Message);
    }
}
