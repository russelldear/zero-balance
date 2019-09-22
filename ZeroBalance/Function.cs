using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Slight.Alexa.Framework.Models.Requests;
using Slight.Alexa.Framework.Models.Responses;
using ZeroBalance.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace ZeroBalance
{
    public class Function
    {
        private IXeroService _xeroService;

        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            return GetFunctionResponse(input, context, new XeroService(input.Session.User.AccessToken));
        }

        public SkillResponse GetFunctionResponse(SkillRequest input, ILambdaContext context, IXeroService xeroService)
        {
            _xeroService = xeroService;

            LambdaLogger.Log($"SkillRequest: {JsonConvert.SerializeObject(input.Request)}");
            LambdaLogger.Log($"User: {JsonConvert.SerializeObject(input.Session.User)}");

            Response response = new Response();

            IOutputSpeech innerResponse = null;

            var requestType = input.Request.Type;

            if (input.Request.Type == Constants.LaunchRequest)
            {
                LambdaLogger.Log($"Default LaunchRequest made");

                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text = Constants.DefaultLaunchRequestText;

                response.ShouldEndSession = false;
            }
            else if (input.Request.Type == Constants.IntentRequest)
            {
                var intent = input.Request.Intent.Name;

                LambdaLogger.Log($"Intent Requested {intent}");

                var responseText = "";

                if (intent == Constants.ConnectionsIntent)
                {
                    try
                    {
                        responseText = _xeroService.GetConnections();
                    }
                    catch (UnauthorisedException)
                    {
                        responseText = Constants.UnauthorisedResponse;
                        response.Card = new LinkAccountCard();
                    }

                    response.ShouldEndSession = true;
                }
                else if (intent == Constants.VersionIntent)
                {
                    responseText = $"Deployed version is {context.FunctionVersion}";
                    response.ShouldEndSession = true;
                }
                else if (intent == Constants.HelpIntent)
                {
                    responseText = Constants.HelpRequestText;
                    response.ShouldEndSession = false;
                }
                else if (intent == Constants.StopIntent)
                {
                    response.ShouldEndSession = true;
                }
                else if (intent == Constants.CancelIntent)
                {
                    response.ShouldEndSession = true;
                }

                LambdaLogger.Log($"Response: {responseText} - Session should end: {response.ShouldEndSession}");

                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text = responseText;
            }

            response.OutputSpeech = innerResponse;

            var skillResponse = new SkillResponse
            {
                Response = response,
                Version = "1.0"
            };

            return skillResponse;
        }
    }
}