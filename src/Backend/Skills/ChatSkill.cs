using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.TemplateEngine;
using System.ComponentModel;

namespace Backend.Skills
{
    public class AdvancedChatSkill
    {
        private IKernel _kernel;

        public AdvancedChatSkill(IKernel kernel)
        {
            _kernel = kernel;
        }

        [SKFunction, Description("Get a chat response")]
        public async Task<SKContext> ChatAsync([Description("The new message")] string input, SKContext context)
        {
            var transliterationToUrdu = await ConvertTransliterationToUrdu(input);

            context["Transliteration"] = transliterationToUrdu;
            var response = await GetResponseAsync(context);

            context.Variables.Update(response);

            return context;
        }


        [SKFunction, Description("Converts transliterated urdu into the urdu script")]
        public async Task<string> ConvertTransliterationToUrdu([Description("The message to convert")]string input)
        {
            const string skPrompt = @"The following input is a sentence that a student has said to their teacher, it should be in urdu and might be written as transliterated urdu.

If it is transliterated urdu convert it into the urdu script, if the sentence is already in the urdu script return it exactly as it is already written. If the input isn't urdu, either transliterated or urdu script then return the sentence as is.

Sententence to convert: {{$input}}
Converted:";

            var function = _kernel.CreateSemanticFunction(skPrompt, maxTokens: 500, temperature: 0, topP: 0.5);
            var response = await function.InvokeAsync(input);

            return response.Result;
        }

        private async Task<string> GetResponseAsync(SKContext context)
        {
            const string skPrompt = @"You are an urdu language teacher. Today you are having a conversation class with your student.
You will only respond in urdu, using the urdu script. Your student is a beginner so your responses should be short and simple. 
When the student says hello, start a simple conversation in urdu with them.

{{$history}}
ChatBot:";

            var function = _kernel.CreateSemanticFunction(skPrompt, maxTokens: 2000, temperature: 0.7, topP: 0.5);
            var response = await function.InvokeAsync();

            return response.Result;
        }

        //[SKFunction("Get chat response")]
        //[SKFunctionName("Chat")]
        //[SKFunctionInput(Description = "The new message")]
        //[SKFunctionContextParameter(Name = "userId", Description = "Unique and persistent identifier for the user")]
        //[SKFunctionContextParameter(Name = "userName", Description = "Name of the user")]
        //[SKFunctionContextParameter(Name = "chatId", Description = "Unique and persistent identifier for the chat")]
        //[SKFunctionContextParameter(Name = "proposedPlan", Description = "Previously proposed plan that is approved")]
        //[SKFunctionContextParameter(Name = "messageType", Description = "Type of the message")]
        //[SKFunctionContextParameter(Name = "responseMessageId", Description = "ID of the response message for planner")]
        //public async Task<SKContext> ChatAsync(string message, SKContext context)
        //{
        //    //var userId = context["userId"];
        //    //var userName = context["userName"];
        //    //var chatId = context["chatId"];
        //    //var messageType = context["messageType"];



        //    context.Variables.Update(response);
        //    return context;
        //}
    }
}
