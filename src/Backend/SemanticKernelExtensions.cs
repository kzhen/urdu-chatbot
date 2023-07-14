using Backend.Chat;
using Backend.Configuration;
using Backend.Skills;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SemanticFunctions;
using Microsoft.SemanticKernel.Skills.Core;

namespace Backend
{
    internal static class SemanticKernelExtensions
    {
        private delegate Task RegisterSkillsWithKernel(IServiceProvider sp, IKernel kernel);

        internal static IServiceCollection AddSemanticKernelServices(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddOptions<AzureOpenAIOptions>()
                .Bind(configuration.GetSection(AzureOpenAIOptions.PropertyName))
                .ValidateOnStart();

            services.AddSingleton<IChatHistoryRespository, ChatHistoryRepository>();
            services.AddScoped<IKernel>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value;

                var kernel = new KernelBuilder()
                    .WithLogger(sp.GetRequiredService<ILogger<IKernel>>())
                    .WithAzureChatCompletionService(options.Models.Completion, options.Endpoint, options.ApiKey)
                    .WithAzureTextEmbeddingGenerationService(options.Models.Embedding, options.Endpoint, options.ApiKey)
                    .Build();

                sp.GetRequiredService<RegisterSkillsWithKernel>()(sp, kernel);

                return kernel;
            });

            services.AddScoped<RegisterSkillsWithKernel>(sp => RegisterSkillsAsync);

            return services;
        }

        private static Task RegisterSkillsAsync(IServiceProvider sp, IKernel kernel)
        {
            kernel.ImportSkill(new AdvancedChatSkill(
                kernel: kernel)
                , nameof(AdvancedChatSkill));

            RegisterSimpleChatSkill(kernel);
            RegisterSimpleTranslationSkill(kernel);
            kernel.ImportSkill(new TimeSkill(), nameof(TimeSkill));

            return Task.CompletedTask;
        }

        private static void RegisterSimpleTranslationSkill(IKernel kernel)
        {
            const string skPrompt = @"You are a urdu language teacher and have just said the following sentence to your student

{{$input}}

They have been unable to understand this sentence, please translate it into English

Translation:";

            var promptConfig = new PromptTemplateConfig
            {
                Completion =
                {
                    MaxTokens = 2000,
                    Temperature = 0.7,
                    TopP = 0.5,
                }
            };

            var promptTemplate = new PromptTemplate(skPrompt, promptConfig, kernel);
            var functionConfig = new SemanticFunctionConfig(promptConfig, promptTemplate);
            var chatFunction = kernel.RegisterSemanticFunction("ChatBot", "Translate", functionConfig);
        }

        private static void RegisterSimpleChatSkill(IKernel kernel)
        {
            const string skPrompt = @"You are a urdu language teacher. Today you are having a conversation class with your student.
You will only respond in urdu, using the urdu script. Your student is a beginner so your responses should be short and simple. 
When the student says hello, start a simple conversation in urdu with them.

{{$history}}
ChatBot:";

            var promptConfig = new PromptTemplateConfig
            {
                Completion =
                {
                    MaxTokens = 2000,
                    Temperature = 0.7,
                    TopP = 0.5,
                }
            };

            var promptTemplate = new PromptTemplate(skPrompt, promptConfig, kernel);
            var functionConfig = new SemanticFunctionConfig(promptConfig, promptTemplate);
            var chatFunction = kernel.RegisterSemanticFunction("ChatBot", "Chat", functionConfig);
        }
    }
}
