using Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cli;

public class ApplicationCliService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHostApplicationLifetime _lifetime;

    public ApplicationCliService(IServiceScopeFactory serviceScopeFactory, IHostApplicationLifetime lifetime)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _lifetime = lifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var applicationOrchestrator = scope.ServiceProvider.GetRequiredService<IApplicationOrchestrator>();

        await applicationOrchestrator.Synchronize();

        _lifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
