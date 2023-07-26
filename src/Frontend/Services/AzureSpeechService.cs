using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Net.Http.Json;

namespace Frontend.Services
{
    public class AzureSpeechService
    {
        private HttpClient _httpClient;
        public async Task<SpeechSynthesizer> CreateSpeechSynthesizer()
        {
            var token = await GetToken();
            var config = SpeechConfig.FromAuthorizationToken(token, "uksouth");
            
            return new SpeechSynthesizer(config);
        }

        public AzureSpeechService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetToken()
        {
            _httpClient.BaseAddress = new Uri("https://localhost:7071");
            var response = await _httpClient.GetFromJsonAsync<TokenResponse>("/api/speech/token");

            return response!.Token;
        }

        private class TokenResponse
        {
            public string Token { get; set; } = string.Empty;
            public string Region { get; set; } = string.Empty;
        }
    }
}
