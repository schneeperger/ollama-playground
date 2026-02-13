using Microsoft.Extensions.DependencyInjection;
using OllamaPlayground.Services;
using OllamaPlayground.Models.Chat;

var services = new ServiceCollection();

services.AddHttpClient<IOllamaService, OllamaService>(client =>
{
  client.BaseAddress = new Uri("http://localhost:11434");
});

var provider = services.BuildServiceProvider();

var ollama = provider.GetRequiredService<IOllamaService>();



// var messages = new List<OllamaMessage>
// {
//     new() { Role = "system", Content = "You're a helpful assitent."},
//     new() { Role = "user", Content = "Hello, how are you?"}
// };

// var reply1 = await ollama.ChatAsync(messages);
// Console.WriteLine("Assistant: " + reply1);

// messages.Add(new OllamaMessage { Role = "assistant", Content = reply1 });
// messages.Add(new OllamaMessage { Role = "user", Content = "What type of car should I buy if I need to use it as a boat?"});

// var reply2 = await ollama.ChatAsync(messages);
// Console.WriteLine("Assistant: " + reply2);

// var answer = await ollama.AskAsync("what is the meaning of life?");

// Console.WriteLine(answer);

var history = new List<OllamaMessage>
{
    new() { Role = "user", Content = "Hello, who are you?" }
};

await foreach (var token in ollama.ChatStreamAsync(history))
{
    Console.Write(token);
    await Task.Delay(70);
}

Console.WriteLine("\nDone.");
