using System;
using System.IO;
using CookieMaker.RabbitMQEventBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CookieMaker.CookieControl
{
    class Program
    {
        public static readonly string Namespace = typeof(Program).Namespace;
        public static readonly string AppName = Namespace.Substring(Namespace.LastIndexOf('.') + 1);

        static void Main(string[] args)
        {
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

            string read;

            do
            {
                Console.WriteLine("Write as command:");
                Console.WriteLine("q - quit");
                Console.WriteLine("[number] - queue cookies");
                Console.WriteLine("g - get stock");

                read = Console.ReadLine();

                if(read == "g")
                {
                    application.QueryCookies();
                }
                else
                {
                    int cookies;
                    if(int.TryParse(read, out cookies))
                    {
                        application.QueueCookies(cookies);
                    }
                }

            }
            while(read != "q");

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
