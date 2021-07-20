using Fulfilment.Core.Configuration;
using Fulfilment.Core.Tracing;
using Fulfilment.Web.Models;
using Fulfilment.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Fulfilment.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger _logger;
        private readonly ListDocumentsService _documentsService;
        private readonly ActivitySource _activitySource;
        private readonly ObservabilityOptions _options;

        public IEnumerable<Document> Documents { get; private set; }
        public bool CallFailed { get; private set; }

        [BindProperty]
        public string UserId { get; set; }

        public IndexModel(ListDocumentsService documentsService, ILogger<IndexModel> logger, ActivitySource activitySource, ObservabilityOptions options)
        {
            _documentsService = documentsService;
            _logger = logger;
            _activitySource = activitySource;
            _options = options;
        }

        public async Task<IActionResult> OnGet()
        {
            Baggage.AddFromIncoming(Request.Headers);
            if (Request.Query.ContainsKey("all"))
            {
                await LoadDocuments(all: true);
            }
            else if (Request.Query.ContainsKey("userId"))
            {
                UserId = Request.Query["userId"];
                await LoadDocuments();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Baggage.AddFromIncoming(Request.Headers);
            await LoadDocuments();
            return Page();
        }

        private async Task LoadDocuments(bool all = false)
        {
            var transactionId = Guid.NewGuid().ToString();
            Activity postSpan = null;
            if (_options.Trace.CustomSpans)
            {
                postSpan = _activitySource.StartActivity("list-documents");
                postSpan.AddTag("span.kind", "internal")
                        .AddTag("user.id", UserId)
                        .AddBaggage("transaction.id", transactionId);
            }

            try
            {
                if (all)
                {
                    _logger.LogDebug("Loading all documents");
                    Documents = await _documentsService.GetDocuments();
                    CallFailed = false;
                    _logger.LogInformation("Loaded documents: {DocumentCount}", Documents.Count());
                }
                else
                {
                    _logger.LogDebug("Loading documents for user ID: {UserId}", UserId);
                    Documents = await _documentsService.GetDocuments(UserId);
                    CallFailed = false;
                    _logger.LogInformation("Loaded documents: {DocumentCount}; user ID: {UserId}", Documents.Count(), UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("API call failed: {Exception}", ex);
                Documents = null;
                CallFailed = true;
            }
            finally
            {
                if (postSpan != null)
                {
                    postSpan.Dispose();
                }
            }
        }
    }
}
