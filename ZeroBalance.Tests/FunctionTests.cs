using Xunit;
using Moq;
using Amazon.Lambda.Core;
using Slight.Alexa.Framework.Models.Requests;
using Slight.Alexa.Framework.Models.Responses;

namespace ZeroBalance.Tests
{
    public class FunctionTests
    {
        private const string SuccessfulResponse = "Successful response.";

        private Mock<SkillRequest> _mockSkillRequest;
        private Mock<ILambdaContext> _mockLambdaContext;
        private Mock<IXeroService> _mockXeroService;
        private SkillResponse _skillResponse;

        [Fact]
        public void can_trigger_default_launch_request()
        {
            Given_a_launch_request();

            When_I_run_the_skill_request();

            Then_response_text_is_this(Constants.DefaultLaunchRequestText);
        }

        [Fact]
        public void can_trigger_connections_intent()
        {
            Given_an_intent_request_with_this_name(Constants.ConnectionsIntent);

            When_I_run_the_skill_request();

            Then_response_text_is_this(SuccessfulResponse);
        }

        [Fact]
        public void can_trigger_help_intent()
        {
            Given_an_intent_request_with_this_name(Constants.HelpIntent);

            When_I_run_the_skill_request();

            Then_response_text_is_this(Constants.HelpRequestText);
        }

        [Fact]
        public void can_trigger_stop_intent()
        {
            Given_an_intent_request_with_this_name(Constants.StopIntent);

            When_I_run_the_skill_request();

            Then_session_is_ended();
        }

        [Fact]
        public void can_trigger_cancel_intent()
        {
            Given_an_intent_request_with_this_name(Constants.CancelIntent);

            When_I_run_the_skill_request();

            Then_session_is_ended();
        }

        [Fact]
        public void unauthorised_user_gets_link_account_card()
        {
            Given_an_unauthorised_intent_request();

            When_I_run_the_skill_request();

            Then_response_text_is_this(Constants.UnauthorisedResponse);

            Then_a_link_account_card_is_returned();
        }

        private void Given_a_launch_request()
        {
            _mockSkillRequest = new Mock<SkillRequest>();
            _mockLambdaContext = new Mock<ILambdaContext>();
            _mockXeroService = new Mock<IXeroService>();

            _mockSkillRequest.Object.Request = new Slight.Alexa.Framework.Models.Requests.RequestTypes.RequestBundle
            {
                Type = Constants.LaunchRequest
            };

            _mockSkillRequest.Object.Session = new Session { User = new User() };
        }

        private void Given_an_intent_request_with_this_name(string name)
        {
            _mockSkillRequest = new Mock<SkillRequest>();
            _mockLambdaContext = new Mock<ILambdaContext>();
            _mockXeroService = new Mock<IXeroService>();

            _mockSkillRequest.Object.Request = new Slight.Alexa.Framework.Models.Requests.RequestTypes.RequestBundle
            {
                Type = Constants.IntentRequest,
                Intent = new Intent { Name = name }
            };

            _mockSkillRequest.Object.Session = new Session { User = new User() };

            _mockXeroService.Setup(s => s.GetConnections()).Returns(SuccessfulResponse);
        }

        private void Given_an_unauthorised_intent_request()
        {
            _mockSkillRequest = new Mock<SkillRequest>();
            _mockLambdaContext = new Mock<ILambdaContext>();
            _mockXeroService = new Mock<IXeroService>();

            _mockSkillRequest.Object.Request = new Slight.Alexa.Framework.Models.Requests.RequestTypes.RequestBundle
            {
                Type = Constants.IntentRequest,
                Intent = new Intent { Name = "ConnectionsIntent" }
            };

            _mockSkillRequest.Object.Session = new Session { User = new User() };

            _mockXeroService.Setup(s => s.GetConnections()).Throws(new UnauthorisedException());
        }

        private void When_I_run_the_skill_request()
        {
            var e = new Function();

            _skillResponse = e.GetFunctionResponse(_mockSkillRequest.Object, _mockLambdaContext.Object, _mockXeroService.Object);

            Assert.NotNull(_skillResponse);
        }

        private void Then_session_is_ended()
        {
            Assert.True(_skillResponse.Response.ShouldEndSession);
        }

        private void Then_response_text_is_this(string expectedResponseText)
        {
            Assert.Equal(expectedResponseText, ((PlainTextOutputSpeech)_skillResponse.Response.OutputSpeech).Text);
        }

        private void Then_a_link_account_card_is_returned()
        {
            Assert.True(_skillResponse.Response.Card is LinkAccountCard);
        }
    }
}
