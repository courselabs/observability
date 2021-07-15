using Fulfilment.Web.Model;
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
        private readonly string _authzUrl;

        public DocumentsService(IHttpClientFactory clientFactory, IConfiguration config)
        {
            _clientFactory = clientFactory;
            _config = config;

            ApiUrl = _config["Documents:Api:Url"];
            _authzUrl = _config["Documents:Authz:Url"];
        }

        public async Task<IEnumerable<Document>> GetDocuments(string userId)
        {
            var client = _clientFactory.CreateClient();
            var authzResult = new AuthorizationResult
            {
                IsAllowed = true
            };

            if (_authzUrl != string.Empty)
            {
                var authzResponse = await client.GetAsync($"{_authzUrl}/{userId}/{DocumentAction.List}");
                if (!authzResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Authz call failed, status: {authzResponse.StatusCode}, message: {authzResponse.ReasonPhrase}");
                }
                using var contentStream = await authzResponse.Content.ReadAsStreamAsync();
                authzResult = await JsonSerializer.DeserializeAsync<AuthorizationResult>(contentStream, _jsonOptions);
            }

            if (!authzResult.IsAllowed)
            {
                throw new Exception($"Not authorized! User: {userId}, action: {DocumentAction.List}");
            }
            
            var response = await client.GetAsync(ApiUrl);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API call failed, status: {response.StatusCode}, message: {response.ReasonPhrase}");
            }
            using (var contentStream = await response.Content.ReadAsStreamAsync())
            {
                return await JsonSerializer.DeserializeAsync<IEnumerable<Document>>(contentStream, _jsonOptions);
            }
        }
    }
}
