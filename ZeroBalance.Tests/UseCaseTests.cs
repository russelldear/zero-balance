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

            Then_response_text_is_this("You have connected the following organisations to Zero Balance: Something");
        }

        private void Given_a_valid_connection()
        {
            _mockSkillRequest = new Mock<SkillRequest>();
            _mockLambdaContext = new Mock<ILambdaContext>();

            _mockSkillRequest.Object.Request = new RequestBundle
            {
                Type = Constants.IntentRequest,
                Intent = new Intent { Name = "ConnectionsIntent" }
            };

            _mockSkillRequest.Object.Session = new Session { User = new User() };

            _fakeHttpClient = new FakeHttpClient(HttpStatusCode.OK);

            _xeroService = new XeroService(_fakeHttpClient);
        }

        private void When_I_request_my_connections()
        {
            _response = new Function().GetFunctionResponse(_mockSkillRequest.Object, _mockLambdaContext.Object, _xeroService);
        }

        private void Then_response_text_is_this(string expectedResponseText)
        {
            Assert.Contains(expectedResponseText, ((PlainTextOutputSpeech)_response.Response.OutputSpeech).Text);
        }
    }
}
