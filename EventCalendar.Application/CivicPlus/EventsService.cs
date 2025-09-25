using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EventCalendar.Application.Contracts.CivicPlus;
using EventCalendar.Application.Contracts.CivicPlus.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;

namespace EventCalendar.Application.CivicPlus
{
    public class EventsService:IEventsService
    {
        private readonly HttpClient _httpClient;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;
        private string _baseUrl = string.Empty;
        private string _authEndpoint = string.Empty;
        private string _eventsEndpoint = string.Empty;
        public EventsService(HttpClient httpClient, IDistributedCache cache, IConfiguration config)
        {
            _httpClient = httpClient;
            _cache = cache;
            _config = config;
            _baseUrl = _config["CivicPlusService:BaseUrl"] ?? throw new ArgumentNullException("CivicPlusService:BaseUrl configuration is missing");
            _authEndpoint = _config["CivicPlusAuth:AuthEndpoint"] ?? throw new ArgumentNullException("CivicPlusAuth:AuthEndpoint configuration is missing");
            _eventsEndpoint = _config["CivicPlusAuth:EventsEndpoint"] ?? throw new ArgumentNullException("CivicPlusAuth:EventsEndpoint configuration is missing");
        }
        
        public async Task<EventsDto> CreateEvent(EventsDto events)
        {
            var token = await GetEventsTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var json = JsonSerializer.Serialize(events);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_baseUrl + _eventsEndpoint, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<EventsDto>(responseBody);

            if (result == null)
            {
                throw new Exception("Failed to deserialize event from the response.");
            }

            return result;
        }

        public async Task<EventsDto?> GetEventByIdAsync(Guid eventId)
        {
            throw new NotImplementedException();
        }

        public async Task<EventResponseDto> GetEventsAsync(Dictionary<string, string> queryParams)
        {            
            var token = await GetEventsTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));

            var url = QueryHelpers.AddQueryString(_baseUrl + _eventsEndpoint, queryParams);            

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<EventResponseDto>(
                responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            if (result == null)
            {
                throw new Exception("Failed to deserialize events from the response.");
            }

            return result;
        }

        private async Task<string> GetEventsTokenAsync()
        {
            var cacheKey = "events_jwt_token";
            var token = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(token))
                return token;

            var clientId = _config["CivicPlusAuth:ClientId"];
            var clientSecret = _config["CivicPlusAuth:ClientSecret"];

            var payload = new
            {
                clientId = clientId,
                clientSecret = clientSecret
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_baseUrl + _authEndpoint, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<EventsTokenDto>(responseBody);
            

            token = result?.access_token ?? throw new Exception("Token not found in response.");

            // Store token in Redis with expiry
            var expiresInSeconds = result.expires_in > 0 ? result.expires_in : 3600;
            await _cache.SetStringAsync(cacheKey, token, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiresInSeconds)
            });

            return token;
        }
    }
}
