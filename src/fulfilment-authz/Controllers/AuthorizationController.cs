using Fulfilment.Authorization.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fulfilment.Authorization.Controllers
{
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly string _idpUrl;

        public AuthorizationController(IHttpClientFactory clientFactory, IConfiguration config, ILogger<AuthorizationController> logger)
        {
            _clientFactory = clientFactory;
            _config = config;
            _logger = logger;
            _idpUrl = _config["IdentityProvider:Url"];
        }

        [HttpGet("/check/{userId}/{documentAction}")]
        public async Task<IActionResult> Check(string userId, DocumentAction documentAction)
        {
            if (Activity.Current != null)
            {
                _logger.LogDebug("Auth check request ID: {RequestId}, from source: {AuthCheckSource}", 
                    Activity.Current.GetBaggageItem("request-id"),
                    Activity.Current.GetBaggageItem("request-source"));
            }

            //not a real idp call - just used to create another span in the trace:
            if (_idpUrl != string.Empty)
            {
                _logger.LogDebug("Making identity provider call, URL: {IdpUrl}", _idpUrl);
                var client = _clientFactory.CreateClient();
                await client.GetAsync(_idpUrl);
            }

            var result = new AuthorizationResult
            {
                UserId = userId,
                Action = documentAction
            };

            result.IsAllowed = (documentAction == DocumentAction.List && userId.StartsWith("0")) ||
                               (documentAction == DocumentAction.Submit && userId.StartsWith("04")) ||
                               (documentAction == DocumentAction.Delete && userId.StartsWith("041"));

            return Ok(result);
        }
    }
}
