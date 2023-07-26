using Backend.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Backend.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SpeechController : ControllerBase
    {
        private readonly AzureSpeechOptions _speechOptions;
        private readonly HttpClient _httpClient;

        public SpeechController(IOptions<AzureSpeechOptions> speechOptions, HttpClient httpClient)
        {
            _speechOptions = speechOptions.Value;
            _httpClient = httpClient;
        }

        [HttpGet]
        [Route("token")]
        public async Task<IActionResult> GetToken()
        {
            var request = new HttpRequestMessage
            {

                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://{_speechOptions.Region}.api.cognitive.microsoft.com/sts/v1.0/issueToken")
            };
            request.Headers.Add("Ocp-Apim-Subscription-Key", _speechOptions.ApiKey);

            var response = await _httpClient.SendAsync(request);

            var token = await response.Content.ReadAsStringAsync();

            return Ok(new { token, region = _speechOptions.Region });
        }
    }
}
