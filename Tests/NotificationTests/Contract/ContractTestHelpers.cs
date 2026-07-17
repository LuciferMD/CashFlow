using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Notification.Hubs;
using Notification.Models;
using Notification.Services;

namespace NotificationTests.Contract;

internal static class ContractTestHelpers
{
    internal static string PactsDirectory => ContractPaths.PactsDirectory;

    internal static string NotificationGatewayPactPath => ContractPaths.NotificationGatewayPact;

    internal static SnapshotProcessor CreateProcessor()
    {
        var mockHub = new Mock<IHubContext<NotificationHub>>();
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockHub.Setup(x => x.Clients).Returns(mockClients.Object);
        mockClients.Setup(x => x.All).Returns(mockClientProxy.Object);
        mockClientProxy
            .Setup(x => x.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockTelegram = new Mock<ITelegramService>();
        mockTelegram
            .Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new SnapshotProcessor(
            Options.Create(new NotificationOptions { HumidityThreshold = 70 }),
            mockHub.Object,
            mockTelegram.Object,
            NullLogger<SnapshotProcessor>.Instance);
    }
}
