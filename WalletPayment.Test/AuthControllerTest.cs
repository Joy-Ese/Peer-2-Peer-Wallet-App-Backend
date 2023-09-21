using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WalletPayment.Api.Controllers;
using WalletPayment.Services.Data;
using WalletPayment.Services.Interfaces;
using WalletPayment.Services.Services;

namespace WalletPayment.Test
{
    public class AuthControllerTest
    {
        AuthService _authService;
        AuthController _authController;

        public AuthControllerTest()
        {
            var dbOptions = new DbContextOptions<DataContext>();
            var context = new DataContext(dbOptions);
            var configuration = new ConfigurationBuilder().Build();
            var httpContextAccessor = new HttpContextAccessor();
            var linkGenerator = new LinkGenerator();
            var webHostEnvironment = new Mock<IWebHostEnvironment>().Object;
            var authServiceLogger = new Logger<AuthService>(new NullLoggerFactory());
            var emailServiceLogger = new Logger<EmailService>(new NullLoggerFactory());
            var hubContext = new Mock<IHubContext<NotificationSignalR>>().Object;

            var emailService = new EmailService(context, configuration, httpContextAccessor, emailServiceLogger);

            _authService = new AuthService(context, emailService, configuration, httpContextAccessor, linkGenerator, authServiceLogger, webHostEnvironment, hubContext);
            _authController = new AuthController(context, _authService);
        }

        [Fact]
        public void Register_ExpectedBehaviour()
        {
            // Arrange
            var mockAuthService = new Mock<AuthService>();
            //mockAuthService.Setup(x => x.DoSomething()).Returns("Mocked result");

            // Act
            //var result = mockAuthService.Object.DoSomething();

            // Assert
            //Assert.Equal("Mocked result", result);
        }
    }
}