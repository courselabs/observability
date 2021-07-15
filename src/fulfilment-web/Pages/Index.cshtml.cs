using Fulfilment.Web.Models;
using Fulfilment.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
            try
            {                
                Documents = await _documentsService.GetDocuments("0421");
                CallFailed = false;
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
