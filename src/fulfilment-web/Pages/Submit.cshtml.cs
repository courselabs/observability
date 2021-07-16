using Fulfilment.Web.Configuration;
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

        public async Task<IActionResult> OnPostAsync()
        {
            var requestId = Guid.NewGuid().ToString();
            Activity postSpan = null;
            if (_options.Trace.CustomSpans)
            {
                postSpan = _activitySource.StartActivity("submit-document");
                postSpan.AddTag("span.kind", "internal")
                        .AddTag("user.id", UserId)
                        .AddTag("request.id", requestId)
                        .AddTag("document.filename", Filename)
                        .AddBaggage("request.id", requestId);
            }

            try
            {
                Document = await _submitService.SubmitDocument(UserId, Filename);
            }
            catch (Exception ex)
            {
                _logger.LogError($"API call failed: {ex}");
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
