using System;
using CookieMaker.RabbitMQEventBus.Messages;
using CookieMaker.RabbitMQEventBus.Responses;
using EasyNetQ;
using Microsoft.Extensions.Logging;

namespace CookieMaker.RecipeManager
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
            this._log.LogDebug("Starting...");

            this._bus.Subscribe<StartRecipeMessage>("RecipeManager_StartRecipeMessage_Subscription", HandleStartRecipeMessage);

            this._log.LogInformation("Listening for messages.");
        }

        private void HandleStartRecipeMessage(StartRecipeMessage msg)
        {
            var cookieId = Guid.NewGuid();

            this._bus.Publish(new StartedNewRecipeMessage() { ProductId = cookieId.ToString() });
            this._log.LogInformation($"Processing new cookie: {msg.Recipe} - Id:{cookieId}");

            var numberOfSteps = 2;

            this._bus.Request<StartAddFlourMessage, EmptyResponseMessage>(new StartAddFlourMessage() { });
            
            this._bus.Publish(new RecipeStatusChangeMessage() { ProductId = cookieId.ToString(), Progress = 100 / numberOfSteps });
            
            this._bus.Request<StartAddSugarMessage, EmptyResponseMessage>(new StartAddSugarMessage() { });

            this._bus.Publish(new RecipeStatusChangeMessage() { ProductId = cookieId.ToString(), Progress = 100 });
            this._log.LogInformation($"Processed cookie: {cookieId}");
        }
    }
}