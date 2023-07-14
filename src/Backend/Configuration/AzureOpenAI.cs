using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Backend.Configuration
{
    public sealed class AzureOpenAIOptions
    {
        public const string PropertyName = "AzureOpenAI";

        [Required(AllowEmptyStrings = false)]
        public string Endpoint { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = false)]
        public string ApiKey { get; set; } = string.Empty;
        [Required]
        public ModelTypes Models { get; set; } = new();

        public class ModelTypes
        {
            public string Completion { get; set; } = string.Empty;
            public string Embedding { get; set; } = string.Empty;
        }
    }
}
