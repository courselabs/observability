using Fulfilment.Core.Configuration;
using Fulfilment.Core.Logging;
using Fulfilment.Core.Tracing;
using Fulfilment.Web.Model;
using Fulfilment.Web.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fulfilment.Web.Services
{
    public class ListDocumentsService
    {
        private static readonly Histogram _ApiHistogram = Metrics.CreateHistogram("web_document_call_seconds", "Document API call duration", "action");

        private readonly ILogger _logger;
        private readonly SetupLogger _setupLogger;
        private readonly AuthorizationService _authzService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;
        private readonly ActivitySource _activitySource;
        private readonly ObservabilityOptions _options;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public string ApiUrl { get; private set; }

        public ListDocumentsService(AuthorizationService authzService, IHttpClientFactory clientFactory, IConfiguration config, ActivitySource activitySource, ObservabilityOptions options, ILogger<ListDocumentsService> logger, SetupLogger setupLogger)
        {
            _logger = logger;
            _setupLogger = setupLogger;
            _authzService = authzService;
            _clientFactory = clientFactory;
            _config = config;
            _activitySource = activitySource;
            _options = options;

            ApiUrl = $"{_config["Documents:Api:BaseUrl"]}/documents";
            _setupLogger.LogInformation("doc-list-url", $"Using document list service at: {ApiUrl}");
        }

        public async Task<IEnumerable<Document>> GetDocuments(string userId)
        {
            var authzResult = await _authzService.Check(userId, DocumentAction.List);
            if (!authzResult.IsAllowed)
            {
                throw new Exception($"Not authorized! User: {userId}, action: {DocumentAction.List}");
            }

            return await GetDocuments();
        }

        public async Task<IEnumerable<Document>> GetDocuments()
        {
            Activity apiSpan = null;
            if (_options.Trace.CustomSpans)
            {
                apiSpan = _activitySource.StartActivity("document-api-call");
                if (apiSpan != null)
                {
                    apiSpan.AddTag("span.kind", "internal")
                           .AddTag("action.type", $"{DocumentAction.List}");
                }
            }

            ITimer timer = null;
            if (_options.Metrics.Enabled)
            {
                timer = _ApiHistogram.WithLabels($"{DocumentAction.List}").NewTimer();
            }

            try
            {
                var client = _clientFactory.CreateClient("client");
                if (_options.Trace.Baggage.Tag)
                {
                    Baggage.AddToOutgoing(client.DefaultRequestHeaders);
                }
                var response = await client.GetAsync(ApiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API call failed, status: {response.StatusCode}, message: {response.ReasonPhrase}");
                }

                using var contentStream = await response.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<IEnumerable<Document>>(contentStream, _jsonOptions);
            }
            finally
            {
                if (apiSpan != null)
                {
                    apiSpan.Dispose();
                }
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
        }
    }
}
