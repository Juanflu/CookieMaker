using CookieMaker.RabbitMQEventBus.Messages;
using CookieMaker.RabbitMQEventBus.Responses;
using EasyNetQ;
using Microsoft.Extensions.Logging;

namespace CookieMaker.CookieControl
{
    public class Application
    {
        private readonly ILogger<Application> _log;
        private readonly IBus _bus;

        public Application(ILogger<Application> log, IBus bus)
        {
            this._log = log;
            this._bus = bus;
        }

        public void QueueCookies(int numberOfCookies)
        {
            for(int i = 1;i<=numberOfCookies;i++)
            {
                this._bus.Publish(new StartRecipeMessage() { Recipe = Recipes.RegularCookie });
                this._log.LogDebug($"Queued cookie #{i}.");
            }
        }

        public void QueryCookies()
        {
            var response = this._bus.Request<RequestStockMessage, RequestStockResponseMessage>(new RequestStockMessage());
            foreach(var cookie in response.Cookies)
            {
                this._log.LogInformation($"Cookie: {cookie.Id} - Progress: {cookie.Progress}");
            }
        }
    }
}