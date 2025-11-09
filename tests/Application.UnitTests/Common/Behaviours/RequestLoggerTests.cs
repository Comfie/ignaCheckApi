using IgnaCheck.Application.Common.Behaviours;
using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Application.Projects.Commands.CreateProject;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace IgnaCheck.Application.UnitTests.Common.Behaviours;

public class RequestLoggerTests
{
    private Mock<ILogger<CreateProjectCommand>> _logger = null!;
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<CreateProjectCommand>>();
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task ShouldCallGetUserNameAsyncOnceIfAuthenticated()
    {
        _user.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

        var requestLogger = new LoggingBehaviour<CreateProjectCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new CreateProjectCommand { Name = "Test Project" }, new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ShouldNotCallGetUserNameAsyncOnceIfUnauthenticated()
    {
        var requestLogger = new LoggingBehaviour<CreateProjectCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new CreateProjectCommand { Name = "Test Project" }, new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Never);
    }
}
