using Fulfilment.Authorization.Model;
using Fulfilment.Core.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fulfilment.Authorization.Controllers
{
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private static readonly Counter _CheckCounter = Metrics.CreateCounter("authorization_checks_total", "Fulfilment authorization checks", "action", "result");
        private static readonly Histogram _IdpHistogram = Metrics.CreateHistogram("authorization_idp_call_seconds", "Fulfilment Identity Provider call duration", "idp_url");

        private readonly IHttpClientFactory _clientFactory;
        private readonly MetricsOptions _metricsOptions;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly string _idpUrl;

        public AuthorizationController(IHttpClientFactory clientFactory, IOptions<ObservabilityOptions> options, IConfiguration config, ILogger<AuthorizationController> logger)
        {
            _clientFactory = clientFactory;
            _metricsOptions = options.Value.Metrics;
            _config = config;
            _logger = logger;
            _idpUrl = _config["IdentityProvider:Url"];
        }

        [HttpGet("/check/{userId}/{documentAction}")]
        public async Task<IActionResult> Check(string userId, DocumentAction documentAction)
        {
            var result = new AuthorizationResult
            {
                UserId = userId,
                Action = documentAction,
                IsAllowed = true
            };
                
            _logger.LogDebug("Auth check request for user ID: {UserId}; action: {DocumentAction}", userId, documentAction);

            if (Activity.Current != null)
            {
                if (Activity.Current.Baggage.Any(x => x.Key == "authz.skip"))
                {
                    _logger.LogWarning("** Skipping authorization!");
                    return Ok(result);
                }

                _logger.LogDebug("Auth check request ID: {RequestId}, from source: {AuthCheckSource}", 
                    Activity.Current.GetBaggageItem("request-id"),
                    Activity.Current.GetBaggageItem("request-source"));
            }

            //not a real idp call - just used to create another span in the trace:
            var url = _idpUrl;
            var experimentalIdp = (userId == "0479");
            if (experimentalIdp)
            {
                url = "https://identity.sixeyed.com/authn";
                _logger.LogInformation("Using experimental identity provider for configured user. IDP: {IdpUrl}; user ID: {UserId}", url, userId);
            }

            if (url != string.Empty)
            {

                ITimer timer = null;
                if (_metricsOptions.Enabled)
                {
                    timer = _IdpHistogram.WithLabels(url).NewTimer();
                }
                try
                {                    
                    _logger.LogTrace("Making identity provider call, URL: {IdpUrl}", url);
                    if (experimentalIdp)
                    {
                        await Task.Delay(25 * 1000);
                    }
                    var client = _clientFactory.CreateClient();
                    await client.GetAsync(url);
                }
                catch
                {
                    _logger.LogWarning("Identity provider call failed! Defaulting to: {IsAuthorized}. IDP: {IdpUrl}; user ID: {UserId}", result.IsAllowed, url, userId);
                }
                finally
                {
                    if (timer != null)
                    {
                        timer.Dispose();
                    }
                }
            }
                         
            result.IsAllowed = (documentAction == DocumentAction.List && userId.StartsWith("0")) ||
                               (documentAction == DocumentAction.Submit && userId.StartsWith("04")) ||
                               (documentAction == DocumentAction.Delete && userId.StartsWith("041"));

            _logger.LogInformation("Auth check result: {IsAuthorized}; user ID: {UserId}; action: {DocumentAction}", result.IsAllowed, userId, documentAction);

            if (_metricsOptions.Enabled)
            {
                _CheckCounter.WithLabels($"{documentAction}", $"{result.IsAllowed}").Inc();
            }

            return Ok(result);
        }
    }
}
