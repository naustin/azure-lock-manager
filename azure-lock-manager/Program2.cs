using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HostConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Create the Host object
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Register your services here
                    services.AddHostedService<ConsoleHostedService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .Build();

            // Run the host
            await host.RunAsync();
        }
    }

    public class ConsoleHostedService : IHostedService
    {
        private readonly ILogger<ConsoleHostedService> _logger;
        private Timer _timer;

        public ConsoleHostedService(ILogger<ConsoleHostedService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Console Hosted Service started.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _logger.LogInformation("Doing some work at: {time}", DateTimeOffset.Now);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Console Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
    }
}
