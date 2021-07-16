using Fulfilment.Core.Configuration;
using Fulfilment.Core.Tracing;
using Fulfilment.Web.Model;
using Fulfilment.Web.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fulfilment.Web.Services
{
    public class SubmitDocumentService
    {
        private readonly AuthorizationService _authzService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;
        private readonly ActivitySource _activitySource;
        private readonly ObservabilityOptions _options;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public string ApiUrl { get; private set; }

        public SubmitDocumentService(AuthorizationService authzService, IHttpClientFactory clientFactory, IConfiguration config, ActivitySource activitySource, ObservabilityOptions options)
        {
            _authzService = authzService;
            _clientFactory = clientFactory;
            _config = config;
            _activitySource = activitySource;
            _options = options;

            ApiUrl = $"{_config["Documents:Api:BaseUrl"]}/document";
        }

        public async Task<Document> SubmitDocument(string userId, string filename)
        {
            var authzResult = await _authzService.Check(userId, DocumentAction.Submit);
            if (!authzResult.IsAllowed)
            {
                throw new Exception($"Not authorized! User: {userId}, action: {DocumentAction.Submit}");
            }

            Activity apiSpan = null;
            if (_options.Trace.CustomSpans)
            {
                apiSpan = _activitySource.StartActivity("document-api-call");
                apiSpan.AddTag("span.kind", "internal")
                       .AddTag("user.id", userId)
                       .AddTag("action.type", $"{DocumentAction.Submit}");
            }

            try
            {
                var json = JsonSerializer.Serialize(new Document
                {
                    FileName = filename,
                    SubmittedByUserId = userId
                }, _jsonOptions);

                var content= new StringContent(json, Encoding.UTF8, "application/json");

                var client = _clientFactory.CreateClient();
                if (_options.Trace.Baggage.Tag)
                {
                    Baggage.AddToOutgoing(client.DefaultRequestHeaders);
                }
                var response = await client.PostAsync(ApiUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API call failed, status: {response.StatusCode}, message: {response.ReasonPhrase}");
                }

                using var contentStream = await response.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<Document>(contentStream, _jsonOptions);
            }
            finally
            {
                if (apiSpan != null)
                {
                    apiSpan.Dispose();
                }
            }
        }
    }
}
