using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Backend.Configuration
{
    public sealed class AzureSpeechOptions
    {
        public const string PropertyName = "AzureSpeech";

        [Required(AllowEmptyStrings = false)]
        public string Region { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = false)]
        public string ApiKey { get; set; } = string.Empty;
    }
}
