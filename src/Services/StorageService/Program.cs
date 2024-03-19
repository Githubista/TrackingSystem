using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StorageService.Extensions;
using StorageService.Settings;

namespace StorageService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    // normally an event log as Serilog needs to be configured e.g builder.UseSerilog
                    logging.AddConsole();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;

                    services.Configure<QueueSettings>(configuration.GetSection(nameof(QueueSettings)));
                    services.Configure<VisitorsFileSettings>(configuration.GetSection(nameof(VisitorsFileSettings)));

                    services.ConfigureMassTransit(configuration);

                    services.AddSingleton<IFileWriter, FileWriter>();
                    services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
                });
    }
}
