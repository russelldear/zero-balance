using System.Net;
using Moq;
using Amazon.Lambda.Core;
using Slight.Alexa.Framework.Models.Requests;
using Slight.Alexa.Framework.Models.Requests.RequestTypes;
using Slight.Alexa.Framework.Models.Responses;
using ZeroBalance.Services;
using Xunit;

namespace ZeroBalance.Tests
{
    public class UseCaseTests
    {
        private FakeHttpClient _fakeHttpClient;
        private XeroService _xeroService;
        private Mock<SkillRequest> _mockSkillRequest;
        private Mock<ILambdaContext> _mockLambdaContext;
        private SkillResponse _response;

        [Fact]
        public void Valid_connection_is_returned()
        {
            Given_a_valid_connection();

            When_I_request_my_connections();

            Then_response_text_contains_this("You have connected the following organisations to Zero Balance: Something");
        }

        [Fact]
        public void Valid_balances_are_returned()
        {
            Given_valid_balances();

            When_I_request_my_balances();

            Then_response_text_contains_this("For organisation");

            Then_response_text_contains_this("Australian dollars");
        }

        [Fact]
        public void Valid_multicurrency_balances_are_returned()
        {
            Given_valid_multicurrency_balances();

            When_I_request_my_balances();

            Then_response_text_contains_this("For organisation");

            Then_response_text_contains_this("Australian dollars");

            Then_response_text_contains_this("Euros");
        }

        private void Given_a_valid_connection()
        {
            _mockSkillRequest = new Mock<SkillRequest>();
            _mockLambdaContext = new Mock<ILambdaContext>();

            _mockSkillRequest.Object.Session = new Session { User = new User() };

            _fakeHttpClient = new FakeHttpClient(HttpStatusCode.OK, false);

            _xeroService = new XeroService(_fakeHttpClient);
        }

        private void Given_valid_balances()
        {
            _mockSkillRequest = new Mock<SkillRequest>();
            _mockLambdaContext = new Mock<ILambdaContext>();

            _mockSkillRequest.Object.Session = new Session { User = new User() };

            _fakeHttpClient = new FakeHttpClient(HttpStatusCode.OK, false);

            _xeroService = new XeroService(_fakeHttpClient);
        }

        private void Given_valid_multicurrency_balances()
        {
            _mockSkillRequest = new Mock<SkillRequest>();
            _mockLambdaContext = new Mock<ILambdaContext>();

            _mockSkillRequest.Object.Session = new Session { User = new User() };

            _fakeHttpClient = new FakeHttpClient(HttpStatusCode.OK, true);

            _xeroService = new XeroService(_fakeHttpClient);
        }

        private void When_I_request_my_connections()
        {
            _mockSkillRequest.Object.Request = new RequestBundle
            {
                Type = Constants.IntentRequest,
                Intent = new Intent { Name = "ConnectionsIntent" }
            };

            _response = new Function().GetFunctionResponse(_mockSkillRequest.Object, _mockLambdaContext.Object, _xeroService);
        }

        private void When_I_request_my_balances()
        {
            _mockSkillRequest.Object.Request = new RequestBundle
            {
                Type = Constants.IntentRequest,
                Intent = new Intent { Name = "BalancesIntent" }
            };

            _response = new Function().GetFunctionResponse(_mockSkillRequest.Object, _mockLambdaContext.Object, _xeroService);
        }

        private void Then_response_text_contains_this(string expectedResponseText)
        {
            Assert.Contains(expectedResponseText, ((PlainTextOutputSpeech)_response.Response.OutputSpeech).Text);
        }
    }
}
