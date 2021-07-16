﻿using Fulfilment.Web.Configuration;
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
            if (Request.Query.ContainsKey("all"))
            {
                await LoadDocuments(all: true);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadDocuments();
            return Page();
        }

        private async Task LoadDocuments(bool all = false)
        {
            var requestId = Guid.NewGuid().ToString();
            Activity postSpan = null;
            if (_options.Trace.CustomSpans)
            {
                postSpan = _activitySource.StartActivity("list-documents");
                postSpan.AddTag("span.kind", "internal")
                        .AddTag("user.id", UserId)
                        .AddTag("request.id", requestId)
                        .AddBaggage("request.id", requestId);
            }

            try
            {
                if (all)
                {
                    _logger.LogDebug("Loading all documents");
                    Documents = await _documentsService.GetDocuments();
                    CallFailed = false;
                    _logger.LogDebug("Loaded documents: {DocumentCount}", Documents.Count());
                }
                else
                {
                    _logger.LogDebug("Loading documents for user ID: {UserId}", UserId);
                    Documents = await _documentsService.GetDocuments(UserId);
                    CallFailed = false;
                    _logger.LogDebug("Loaded documents: {DocumentCount}; user ID: {UserId}", Documents.Count(), UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"API call failed: {ex}");
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
