using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.BlockchainApi.Client;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Lykke.Service.BlockchainApi.Contract.Tests
{
    public class TimeoutTests
    {
        [Fact]
        public async Task Test__NoTimeout__Works()
        {
            var logFactory = new Mock<ILogFactory>();
            var log = new Mock<ILog>();
            logFactory.Setup(x => x.CreateLog(It.IsAny<object>())).Returns(log.Object);
            var handler = new StubHttpMessageHandler((request) =>
            {
                IsAliveResponse isAlive = new IsAliveResponse()
                {
                    Env = "UnitTest",
                    IsDebug = false,
                    Name = $"",
                    Version = "1.0.0"
                };

                var serialized = JsonConvert.SerializeObject(isAlive);
                var responseObj = new HttpResponseMessage(HttpStatusCode.OK);
                responseObj.Content = new StringContent(serialized, Encoding.UTF8, "application/json");

                return responseObj;
            });

            var blockchainApiClient =
                new BlockchainApiClient(logFactory.Object, "http://localhost:5000", 2, TimeSpan.FromSeconds(10), handler);

            var response = await blockchainApiClient.GetIsAliveAsync();
        }

        [Fact]
        public async Task Test__Timeout__Throws()
        {
            var logFactory = new Mock<ILogFactory>();
            var log = new Mock<ILog>();
            logFactory.Setup(x => x.CreateLog(It.IsAny<object>())).Returns(log.Object);
            var handler = new StubHttpMessageHandler((request) =>
            {
                IsAliveResponse isAlive = new IsAliveResponse()
                {
                    Env = "UnitTest",
                    IsDebug = false,
                    Name = $"",
                    Version = "1.0.0"
                };

                Task.Delay(TimeSpan.FromSeconds(10)).Wait();

                var serialized = JsonConvert.SerializeObject(isAlive);
                var responseObj = new HttpResponseMessage(HttpStatusCode.OK);
                responseObj.Content = new StringContent(serialized, Encoding.UTF8, "application/json");

                return responseObj;
            });

            var blockchainApiClient =
                new BlockchainApiClient(logFactory.Object, "http://localhost:5000", 2, TimeSpan.FromSeconds(1), handler);

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                var response = await blockchainApiClient.GetIsAliveAsync();
            });
        }

        [Fact(Skip = "Remove skip flag if you want to check actual behaviour")]
        public async Task Test__TimeoutOnRealApi__Throws()
        {
            var logFactory = new Mock<ILogFactory>();
            var log = new Mock<ILog>();
            logFactory.Setup(x => x.CreateLog(It.IsAny<object>())).Returns(log.Object);

            var blockchainApiClient =
                new BlockchainApiClient(logFactory.Object, "http://stellar-api.bcn.svc.cluster.local", 20, TimeSpan.FromSeconds(1));

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                var response = await blockchainApiClient.GetIsAliveAsync();
            });
        }

        [Fact(Skip = "Remove skip flag if you want to check actual behaviour")]
        public async Task Test__TimeoutOnRealApi__Works()
        {
            var logFactory = new Mock<ILogFactory>();
            var log = new Mock<ILog>();
            logFactory.Setup(x => x.CreateLog(It.IsAny<object>())).Returns(log.Object);

            var blockchainApiClient =
                new BlockchainApiClient(logFactory.Object, "http://stellar-api.bcn.svc.cluster.local", 20);

            var response = await blockchainApiClient.GetIsAliveAsync();
        }
    }

    internal class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _procFunc;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> procFunc)
        {
            _procFunc = procFunc;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = _procFunc(request);
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(response);
        }
    }
}
