using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using CookieMaker.RabbitMQEventBus.DataModel;
using CookieMaker.RabbitMQEventBus.Messages;
using CookieMaker.RabbitMQEventBus.Responses;
using EasyNetQ;
using Microsoft.Extensions.Logging;

namespace CookieMaker.UIApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CookieController : ControllerBase
    {
        private readonly ILogger<CookieController> _log;
        private readonly IBus _bus;

        public CookieController(ILogger<CookieController> log, IBus bus)
        {
            this._log = log;
            this._bus = bus;
        }

        // GET api/Cookie
        [HttpGet]
        public ActionResult<IEnumerable<Cookie>> Get()
        {
            var response = this._bus.Request<RequestStockMessage, RequestStockResponseMessage>(new RequestStockMessage());
            return response.Cookies;
        }

        // GET api/Cookie/5
        [HttpGet("{id}")]
        public ActionResult<Cookie> Get(string id)
        {
            var response = this._bus.Request<RequestStockMessage, RequestStockResponseMessage>(new RequestStockMessage());
            return response.Cookies.Where(c => c.Id == id).SingleOrDefault();
        }

        // POST api/Cookie
        [HttpPost]
        public void Post([FromBody] int numberOfCookies)
        {
            this._log.LogDebug($"CookieController - Post {numberOfCookies}.");

            for(int i = 1;i<=numberOfCookies;i++)
            {
                this._bus.Publish(new StartRecipeMessage() { Recipe = Recipes.RegularCookie });
                this._log.LogDebug($"Queued cookie #{i}.");
            }
        }
    }
}
