using System.Threading.Tasks;
using CookieMaker.RabbitMQEventBus.Messages;
using CookieMaker.RabbitMQEventBus.Responses;
using EasyNetQ;
using Microsoft.Extensions.Logging;

namespace CookieMaker.AddFlourService
{
    public interface IApplication
    {
        void Start();
    }

    public class Application : IApplication
    {
        private readonly ILogger<Application> _log;
        private readonly IBus _bus;

        public Application(ILogger<Application> log, IBus bus)
        {
            this._log = log;
            this._bus = bus;
        }

        public void Start()
        {
            this._log.LogInformation("Starting...");
            this._bus.Respond<StartAddFlourMessage, EmptyResponseMessage>(request => HandleStartAddFlourMessage());
            this._log.LogInformation("Listening for messages.");
        }

        private EmptyResponseMessage HandleStartAddFlourMessage()
        {
            this._log.LogInformation($"Adding flour");
            Task.Delay(1000).Wait();
            this._log.LogInformation($"Flour added");

            return EmptyResponseMessage.EmptyResponse;
        }
    }
}