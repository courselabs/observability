using Fulfilment.Web.Models;
using Fulfilment.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fulfilment.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly DocumentsService _documentsService;

        public IEnumerable<Document> Documents { get; private set; }
        public bool CallFailed { get; private set; }

        public IndexModel(DocumentsService documentsService, ILogger<IndexModel> logger)
        {
            _documentsService = documentsService;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = "0421";
            try
            {
                _logger.LogDebug("Loading documents for user ID: {UserId}", userId);
                Documents = await _documentsService.GetDocuments(userId);
                CallFailed = false; 
                _logger.LogDebug("Loaded documents: {DocumentCount}; user ID: {UserId}", Documents.Count(), userId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"API call failed: {ex}");
                Documents = null;
                CallFailed = true;
            }
            return Page();
        }
    }
}
