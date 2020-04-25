using System;
using System.Linq;
using System.Collections.Generic;
using EasyNetQ;
using CookieMaker.RabbitMQEventBus.DataModel;
using CookieMaker.RabbitMQEventBus.Messages;
using CookieMaker.RabbitMQEventBus.Responses;
using Microsoft.Extensions.Logging;

namespace CookieMaker.StockService
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

        private Dictionary<string, Cookie> cookies = new Dictionary<string, Cookie>();

        public void Start()
        {
            this._log.LogInformation("Starting...");

            this._bus.Subscribe<StartedNewRecipeMessage>(Guid.NewGuid().ToString(), HandleStartedNewRecipeMessage);
            this._bus.Subscribe<RecipeStatusChangeMessage>(Guid.NewGuid().ToString(), HandleRecipeStatusChangeMessage);
            this._bus.Respond<RequestStockMessage, RequestStockResponseMessage>(request => HandleRequestStockMessage());

            this._log.LogInformation("Listening for messages.");
        }

        private void HandleStartedNewRecipeMessage(StartedNewRecipeMessage msg)
        {
            this._log.LogInformation($"StartedNewRecipeMessage received: {msg.ProductId}");

            cookies.Add(msg.ProductId, new Cookie() { Id = msg.ProductId, Progress = 0 });
        }

        private void HandleRecipeStatusChangeMessage(RecipeStatusChangeMessage msg)
        {
            this._log.LogInformation($"RecipeStatusChangeMessage received: {msg.ProductId} - {msg.Progress}%");

            var cookie = this.cookies[msg.ProductId];
            cookie.Progress = msg.Progress;
        }

        private RequestStockResponseMessage HandleRequestStockMessage()
        {
            this._log.LogInformation($"HandleRequestStockMessage received.");

            return new RequestStockResponseMessage() { Cookies = this.cookies.Values.ToList() };
        }
    }
}