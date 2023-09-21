using Moq;

namespace WalletPayment.Tests;

public class UserUnitTest
{
    [Fact]
    public void Register_ExpectedBehaviour()
    {
        // Arrange
        var mockAuthService = new Mock<AuthService>();
        mockAuthService.Setup(x => x.DoSomething()).Returns("Mocked result");

        // Act
        var result = mockAuthService.Object.DoSomething();

        // Assert
        Assert.Equal("Mocked result", result);
    }
}