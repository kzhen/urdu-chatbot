using Backend.Chat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using System.Text.Json;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IKernel _kernel;
        private readonly ILogger<ChatController> _logger;
        private readonly IChatHistoryRespository _chatHistoryRepository;
        private static JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public ChatController(IKernel kernel, ILogger<ChatController> logger, IChatHistoryRespository chatHistoryRespository)
        {
            _kernel = kernel;
            _logger = logger;
            _chatHistoryRepository = chatHistoryRespository;
        }

        [HttpGet]
        [Route("start")]
        public IActionResult Hello()
        {
            return Ok(new AskResult { Result = "Hello" });
        }

        [HttpPost]
        [Route("chat")]
        public async Task<IActionResult> Message([FromBody] Ask model)
        {
            _chatHistoryRepository.Create(model.SessionId, "Ken", model.Input, AuthorRole.User, DateTime.Now);
            var messages = _chatHistoryRepository.FindBySessionId(model.SessionId).OrderByDescending(m => m.Timestamp);
            var historyText = string.Empty;

            foreach (var chatMessage in messages)
            {
                var formattedMessage = chatMessage.ToFormattedString();
                historyText = $"{formattedMessage}\n{historyText}";
            }

            var contextVariables = new ContextVariables(model.Input);
            contextVariables["history"] = historyText;

            var chatSkillFunction = default(ISKFunction);
            var advancedChatSkill = _kernel.Skills.GetFunction("AdvancedChatSkill", "Chat");
            var res = await _kernel.RunAsync(contextVariables, advancedChatSkill!);

            try
            {
                chatSkillFunction = _kernel.Skills.GetFunction("ChatBot", "Chat");
            }
            catch (KernelException ke)
            {
                _logger.LogError(ke, $"Failed to find ChatBot/Chat on server");
                return NotFound($"Failed to find ChatBot/Chat on server");
            }

            var result = await _kernel.RunAsync(contextVariables, chatSkillFunction!);
            _chatHistoryRepository.Create(model.SessionId, "Bot", result.Result, AuthorRole.Assistant, DateTime.Now);

            return Ok(new AskResult
            {
                Result = result.Result,
                SessionId = model.SessionId,
                Transliteration = res.Variables["Transliteration"]
            });
        }

        [HttpPost]
        [Route("translate")]
        public async Task<IActionResult> Translate([FromBody] Translation message)
        {
            var translationSkill = _kernel.Skills.GetFunction("ChatBot", "Translate");
            var contextVariables = new ContextVariables(message.Sentence);
            var result = await _kernel.RunAsync(contextVariables, translationSkill!);

            return Ok(result.Result);
        }
    }

    public class Translation
    {
        public string Sentence { get; set; }
    }

    public class Ask
    {
        public int SessionId { get; set; }
        public string Input { get; set; } = string.Empty;
    }

    public class AskResult
    {
        public int SessionId { get; set; }
        public string Result { get; set; } = string.Empty;
        public string Transliteration { get; set; } = string.Empty;
    }

    //public class ChatSessionMessageModel
    //{
    //    public int SessionId { get; set; }
    //    public string Username { get; set; } = String.Empty;
    //    public string Body { get; set; } = String.Empty;
    //    public bool Mine { get; set; }
    //}
}
