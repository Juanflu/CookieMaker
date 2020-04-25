using System.Threading.Tasks;
using CookieMaker.RabbitMQEventBus.Messages;
using CookieMaker.RabbitMQEventBus.Responses;
using EasyNetQ;
using Microsoft.Extensions.Logging;

namespace CookieMaker.AddSugarService
{
    public interface IApplication
    {
        void Start();
    }

    public class Application : IApplication
    {
        private readonly ILogger<Application> _logger;
        private readonly IBus _bus;

        public Application(ILogger<Application> log, IBus bus)
        {
            this._logger = log;
            this._bus = bus;
        }

        public void Start()
        {
            this._logger.LogInformation("Starting...");
            this._bus.Respond<StartAddSugarMessage, EmptyResponseMessage>(request => HandleStartAddSugarMessage());
            this._logger.LogInformation("Listening for messages.");
        }

        private EmptyResponseMessage HandleStartAddSugarMessage()
        {
            this._logger.LogInformation($"Adding sugar");
            Task.Delay(2000).Wait();
            this._logger.LogInformation($"Sugar added");

            return EmptyResponseMessage.EmptyResponse;
        }
    }
}