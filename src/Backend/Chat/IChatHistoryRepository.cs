using Microsoft.SemanticKernel.AI.ChatCompletion;
using System.Globalization;

namespace Backend.Chat
{
    public interface IChatHistoryRespository
    {
        IEnumerable<ChatMessage> FindBySessionId(int sessionId);
        void Create(int sessionId, string userName, string content, AuthorRole authorRole, DateTime timestamp);
    }

    public class ChatHistoryRepository : IChatHistoryRespository
    {
        Dictionary<int, List<ChatMessage>> chats = new Dictionary<int, List<ChatMessage>>();

        public IEnumerable<ChatMessage> FindBySessionId(int sessionId)
        {
            return chats[sessionId];
        }

        public void Create(int sessionId, string userName, string content, AuthorRole authorRole, DateTime timestamp)
        {

            List<ChatMessage> chat;
            if (!chats.ContainsKey(sessionId))
            {
                chats[sessionId] = new List<ChatMessage>();
            }
            
            chat = chats[sessionId];
            
            chat.Add(new ChatMessage(sessionId, userName, content, authorRole, timestamp));
        }
    }

    public record ChatMessage(int SessionId, string UserName, string Content, AuthorRole AuthorRole, DateTime Timestamp)
    {
        public string ToFormattedString()
        {
            return $"[{Timestamp.ToString("G", CultureInfo.CurrentCulture)}] {UserName}: {Content}";
        }
    }

}
