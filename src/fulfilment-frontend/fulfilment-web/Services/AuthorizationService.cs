using Fulfilment.Core.Configuration;
using Fulfilment.Web.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fulfilment.Web.Services
{
    public class AuthorizationService
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;
        private readonly ActivitySource _activitySource;
        private readonly ObservabilityOptions _options;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        private readonly string _authzUrl;

        public AuthorizationService(IHttpClientFactory clientFactory, IConfiguration config, ActivitySource activitySource, ObservabilityOptions options, ILogger<AuthorizationService> logger)
        {
            _clientFactory = clientFactory;
            _config = config;
            _activitySource = activitySource;
            _options = options;
            _logger = logger;
            _authzUrl = _config["Documents:Authz:Url"];
        }

        public async Task<AuthorizationResult> Check(string userId, DocumentAction action)
        {
            var authzResult = new AuthorizationResult
            {
                IsAllowed = true
            };

            if (string.IsNullOrEmpty(_authzUrl))
            {
                return authzResult;
            }

            Activity authzSpan = null;
            if (_options.Trace.CustomSpans)
            {
                authzSpan = _activitySource.StartActivity("authz-check");
                authzSpan.AddTag("span.kind", "internal")
                         .AddTag("user.id", userId)
                         .AddTag("action.type", $"{action}")
                         .AddBaggage("request.source", "fulfilment-web");
            }

            try
            {
                var client = _clientFactory.CreateClient("client");
                var authzResponse = await client.GetAsync($"{_authzUrl}/{userId}/{action}");
                if (!authzResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Authz call failed, status: {authzResponse.StatusCode}, message: {authzResponse.ReasonPhrase}");
                }
                using var contentStream = await authzResponse.Content.ReadAsStreamAsync();
                authzResult = await JsonSerializer.DeserializeAsync<AuthorizationResult>(contentStream, _jsonOptions);
            }
            finally
            {
                if (authzSpan != null)
                {
                    authzSpan.AddTag("authz.allowed", $"{authzResult.IsAllowed}");
                    authzSpan.Dispose();
                }
            }

            return authzResult;
        }
    }
}
