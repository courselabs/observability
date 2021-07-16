using Fulfilment.Web.Configuration;
using Fulfilment.Web.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fulfilment.Web.Services
{
    public class AuthorizationService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;
        private readonly ActivitySource _activitySource;
        private readonly ObservabilityOptions _options;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        private readonly string _authzUrl;

        public AuthorizationService(IHttpClientFactory clientFactory, IConfiguration config, ActivitySource activitySource, ObservabilityOptions options)
        {
            _clientFactory = clientFactory;
            _config = config;
            _activitySource = activitySource;
            _options = options;
            _authzUrl = _config["Documents:Authz:Url"];
        }

        public async Task<AuthorizationResult> Check(string userId, DocumentAction action)
        {
            var authzResult = new AuthorizationResult
            {
                IsAllowed = true
            };

            if (_authzUrl != string.Empty)
            {
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
                    var client = _clientFactory.CreateClient();
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
            }

            return authzResult;
        }
    }
}
