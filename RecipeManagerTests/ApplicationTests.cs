using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using EasyNetQ;
using System;
using CookieMaker.RabbitMQEventBus.Messages;
using CookieMaker.RabbitMQEventBus.Responses;
using CookieMaker.RecipeManager;
using Microsoft.Extensions.Logging;

namespace CookieMaker.Common.RecipeManagerTests
{
    [TestClass]
    public class ApplicationTests
    {
        private Mock<ILogger<Application>> logMock = new Mock<ILogger<Application>>();

        [TestMethod]
        public void Start_CallsSubscribe()
        {
            var busMock = new Mock<IBus>();

            var app = new Application(logMock.Object, busMock.Object);
            app.Start();

            busMock.Verify(
                x => x.Subscribe<StartRecipeMessage>(
                    It.Is<string>(s => s.Equals("RecipeManager_StartRecipeMessage_Subscription")), 
                    It.IsAny<Action<StartRecipeMessage>>()));
        }
        
        
        private Mock<IBus> ReceiveStartRecipeMessage()
        {
            var busMock = new Mock<IBus>();

            Action<StartRecipeMessage> callback = null;
            busMock.Setup(x => x.Subscribe<StartRecipeMessage>(
                                    It.Is<string>(s => s.Equals("RecipeManager_StartRecipeMessage_Subscription")), 
                                    It.IsAny<Action<StartRecipeMessage>>()))
                                .Callback<string, Action<StartRecipeMessage>>((s, a) => 
                                {
                                    callback = a;
                                });;


            var app = new Application(logMock.Object, busMock.Object);
            app.Start();

            if(callback != null)
            {
                callback(new StartRecipeMessage());
            }

            return busMock;
        }

        [TestMethod]
        public void ReceiveStartRecipeMessage_Publishes_StartedNewRecipeMessage()
        {
            var busMock = this.ReceiveStartRecipeMessage();
            busMock.Verify(x => x.Publish<StartedNewRecipeMessage>(It.IsAny<StartedNewRecipeMessage>()));
        }

        [TestMethod]
        public void ReceiveStartRecipeMessage_Requests_StartAddFlourMessage()
        {
            var busMock = this.ReceiveStartRecipeMessage();
            busMock.Verify(x => x.Request<StartAddFlourMessage, EmptyResponseMessage>(It.IsAny<StartAddFlourMessage>()));
        }

        [TestMethod]
        public void ReceiveStartRecipeMessage_Publishes_RecipeStatusChangeMessageTwice()
        {
            var busMock = this.ReceiveStartRecipeMessage();
            busMock.Verify(x => x.Publish<RecipeStatusChangeMessage>(It.IsAny<RecipeStatusChangeMessage>()), Times.Exactly(2));
        }

        [TestMethod]
        public void ReceiveStartRecipeMessage_Requests_StartAddSugarMessage()
        {
            var busMock = this.ReceiveStartRecipeMessage();
            busMock.Verify(x => x.Request<StartAddSugarMessage, EmptyResponseMessage>(It.IsAny<StartAddSugarMessage>()));
        }
    }
}
