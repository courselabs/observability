using Fulfilment.Core.Configuration;
using Fulfilment.Core.Logging;
using Fulfilment.Core.Tracing;
using Fulfilment.Web.Model;
using Fulfilment.Web.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prometheus;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fulfilment.Web.Services
{
    public class SubmitDocumentService
    {

        private static Random _Random = new Random();
        private static readonly Histogram _ApiHistogram = Metrics.CreateHistogram("web_document_call_seconds", "Document API call duration", "action");
        private static readonly Histogram _RenderHistogram = Metrics.CreateHistogram("web_document_prerender_seconds", "Document pre-render duration",
           new HistogramConfiguration
           {
               LabelNames = new[] { "document_type" },
                // 1.5s buckets, from 1s to 9s
                Buckets = Histogram.LinearBuckets(start: 1, width: 1.5, count: 6)
           });


        private readonly ILogger _logger;
        private readonly SetupLogger _setupLogger;
        private readonly AuthorizationService _authzService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;
        private readonly ActivitySource _activitySource;
        private readonly ObservabilityOptions _options;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private readonly bool _prerender;

        public string ApiUrl { get; private set; }

        public SubmitDocumentService(AuthorizationService authzService, IHttpClientFactory clientFactory, IConfiguration config, ActivitySource activitySource, ObservabilityOptions options, ILogger<SubmitDocumentService> logger, SetupLogger setupLogger)
        {
            _logger = logger;
            _setupLogger = setupLogger;
            _authzService = authzService;
            _clientFactory = clientFactory;
            _config = config;
            _activitySource = activitySource;
            _options = options;

            _prerender = _config.GetValue<bool>("Documents:Prerender");
            ApiUrl = $"{_config["Documents:Api:BaseUrl"]}/document";
            _setupLogger.LogInformation("doc-submit-url", $"Using submit document service at: {ApiUrl}");
        }

        public async Task<Document> SubmitDocument(string userId, string filename)
        {
            var authzResult = await _authzService.Check(userId, DocumentAction.Submit);
            if (!authzResult.IsAllowed)
            {
                throw new Exception($"Not authorized! User: {userId}, action: {DocumentAction.Submit}");
            }

            var document = new Document
            {
                FileName = filename,
                SubmittedByUserId = userId,
                DocType = Path.GetExtension(filename).TrimStart('.').ToLower()
            };

            if (_prerender)
            {
                await PreRender(document);
            }

            return await SubmitDocument(document);
        }

        private async Task PreRender(Document document)
        {
            Activity renderSpan = null;
            if (_options.Trace.CustomSpans)
            {
                renderSpan = _activitySource.StartActivity("document-render");
                if (renderSpan != null)
                {
                    renderSpan.AddTag("span.kind", "internal")
                       .AddTag("user.id", document.SubmittedByUserId)
                       .AddTag("document.type", document.DocType);
                }
            }

            ITimer renderTimer = null;
            if (_options.Metrics.Enabled)
            {
                renderTimer = _RenderHistogram.WithLabels(document.DocType).NewTimer();
            }

            try
            {
                var renderFactor = document.DocType == "pdf" ? 3 : document.DocType == "doc" ? 2 : document.DocType == "docx" ? 0.5 : 1.0;
                var renderMilliSeconds = _Random.Next(1500, 2500) * renderFactor;
                await Task.Delay((int)renderMilliSeconds);
            }
            finally
            {
                if (renderSpan != null)
                {
                    renderSpan.Dispose();
                }
                if (renderTimer != null)
                {
                    renderTimer.Dispose();
                }
            }
        }

        private async Task<Document> SubmitDocument(Document document)
        {
            Activity apiSpan = null;
            if (_options.Trace.CustomSpans)
            {
                apiSpan = _activitySource.StartActivity("document-api-call");
                if (apiSpan != null)
                {
                    apiSpan.AddTag("span.kind", "internal")
                       .AddTag("user.id", document.SubmittedByUserId)
                       .AddTag("action.type", $"{DocumentAction.Submit}");
                }
            }

            ITimer timer = null;
            if (_options.Metrics.Enabled)
            {
                timer = _ApiHistogram.WithLabels($"{DocumentAction.Submit}").NewTimer();
            }

            try
            {
                var json = JsonSerializer.Serialize(document, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

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
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
        }
    }
}
