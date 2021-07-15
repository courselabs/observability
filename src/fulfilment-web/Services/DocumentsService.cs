using Fulfilment.Web.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fulfilment.Web.Services
{
    public class DocumentsService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public string ApiUrl { get; private set; }

        public DocumentsService(IHttpClientFactory clientFactory, IConfiguration config)
        {
            _clientFactory = clientFactory;
            _config = config;
            
            ApiUrl = _config["Documents:Api:Url"];
        }

        public async Task<IEnumerable<Document>> GetDocuments()
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync(ApiUrl);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Service call failed, status: {response.StatusCode}, message: {response.ReasonPhrase}");
            }
            using (var contentStream = await response.Content.ReadAsStreamAsync())
            {
                return await JsonSerializer.DeserializeAsync<IEnumerable<Document>>(contentStream, _jsonOptions);
            }
        }
    }
}
