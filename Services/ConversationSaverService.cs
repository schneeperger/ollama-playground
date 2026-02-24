using OllamaPlayground.Models.Chat;

namespace OllamaPlayground.Services;

public class ConversationSaverService
{
    public string SaveConversation(List<OllamaMessage> messages)
    {
        var now = DateTime.Now;
        var year = now.Year.ToString();
        var month = now.ToString("MMMM"); // Full month name, e.g. "February"
        var day = now.Day.ToString();     // Day as number, e.g. "24"

        var directory = Path.Combine("conversations", year, month, day);
        Directory.CreateDirectory(directory);

        var filename = $"chat-{now:HHmmss}.md";
        var filePath = Path.Combine(directory, filename);

        var content = BuildMarkdown(messages, now);
        File.WriteAllText(filePath, content);

        return filePath;
    }

    private string BuildMarkdown(List<OllamaMessage> messages, DateTime savedAt)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# Chat Session - {savedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        foreach (var message in messages)
        {
            if (message.Role == "system")
                continue;

            var role = message.Role == "user" ? "User" :
                       message.Role == "assistant" ? "Assistant" :
                       char.ToUpper(message.Role[0]) + message.Role[1..];

            sb.AppendLine($"## {role}");
            sb.AppendLine();
            sb.AppendLine(message.Content);
            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine($"*Saved on {savedAt:yyyy-MM-dd HH:mm:ss}*");

        return sb.ToString();
    }
}
