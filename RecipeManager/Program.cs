using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using CookieMaker.RabbitMQEventBus;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.IO;

namespace CookieMaker.RecipeManager
{
    class Program
    {
        public static readonly string Namespace = typeof(Program).Namespace;
        public static readonly string AppName = Namespace.Substring(Namespace.LastIndexOf('.') + 1);

        private static readonly AutoResetEvent _closing = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            try {
                // Configure Logger
                var configuration = GetConfiguration();
                Log.Logger = CreateSerilogLogger(configuration);

                Log.Information("Configuring microservice ({ApplicationContext})...", AppName);

                // Configure Services
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);

                var busHost = Environment.GetEnvironmentVariable("BUS_HOST") ?? "127.0.0.1";
                serviceCollection.AddRabbitMQ(busHost);

                // Start Application
                Log.Information("Starting microservice ({ApplicationContext})...", AppName);
                var serviceProvider = serviceCollection.BuildServiceProvider();
                var application = serviceProvider.GetRequiredService<Application>();
                application.Start();

                // Stall the application until Ctrl+C is presse
                Console.CancelKeyPress += (e,a) => _closing.Set();
                _closing.WaitOne();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Program terminated unexpectedly ({ApplicationContext})!", AppName);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddSerilog())
                    .AddTransient(typeof(Application));
        }

        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var config = builder.Build();

            return builder.Build();
        }

        private static Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
        {
            var seqServerUrl = configuration["Serilog:SeqServerUrl"];
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("ApplicationContext", AppName)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }
    }
}
