using Fulfilment.Core.Configuration;
using Fulfilment.Web.Models;
using Fulfilment.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Fulfilment.Web.Pages
{
    public class SubmitModel : PageModel
    {
        private readonly ILogger _logger;
        private readonly SubmitDocumentService _submitService;
        private readonly ActivitySource _activitySource;
        private readonly ObservabilityOptions _options;

        public Document Document { get; private set; }
        public bool CallFailed { get; private set; }

        [BindProperty]
        public string UserId { get; set; }

        [BindProperty]
        public string Filename { get; set; }

        public SubmitModel(SubmitDocumentService submitService, ILogger<IndexModel> logger, ActivitySource activitySource, ObservabilityOptions options)
        {
            _submitService = submitService;
            _logger = logger;
            _activitySource = activitySource;
            _options = options;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Request.Query.ContainsKey("userId") && Request.Query.ContainsKey("filename"))
            {
                UserId = Request.Query["userId"];
                Filename = Request.Query["filename"];
                return await OnPostAsync();
            }
            else
            {
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var requestId = Guid.NewGuid().ToString();
            Activity postSpan = null;
            if (_options.Trace.CustomSpans)
            {
                postSpan = _activitySource.StartActivity("submit-document");
                if (postSpan != null)
                {
                    postSpan.AddTag("span.kind", "internal")
                        .AddTag("user.id", UserId)
                        .AddTag("request.id", requestId)
                        .AddTag("document.filename", Filename)
                        .AddBaggage("request.id", requestId);
                }
            }

            try
            {
                _logger.LogDebug("Submitting document, filename: {DocumentFilename}; user ID: {UserId}", Filename, UserId);
                Document = await _submitService.SubmitDocument(UserId, Filename);
                _logger.LogInformation("Sumitted document, ID: {DocumentId}; filename: {DocumentFilename}; user ID: {UserId}", Document.Id, Filename, UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError("API call failed: {Exception}", ex);
                Document = null;
                CallFailed = true;
            }
            finally
            {
                if (postSpan != null)
                {
                    postSpan.Dispose();
                }
            }

            return Page();
        }
    }
}
