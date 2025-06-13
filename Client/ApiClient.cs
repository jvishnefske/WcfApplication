using System;
using System.Net.Http;

namespace WcfClient
{
    public static class ApiClient
    {
        private static readonly HttpClient _httpClient;
        // IMPORTANT: Adjust this base address to where your ContactsApi is running.
        // For local development, it's often https://localhost:7001/ or http://localhost:5000/
        // Check your ContactsApi's launchSettings.json or console output when running it.
        private static readonly string _baseAddress = "https://localhost:7001/"; 

        static ApiClient()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(_baseAddress) };
        }

        public static HttpClient Client => _httpClient;
    }
}
