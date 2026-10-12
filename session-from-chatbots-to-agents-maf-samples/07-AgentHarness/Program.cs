// The HarnessAgent comes pre-configured with TodoProvider, AgentModeProvider, FileMemoryProvider,
// ToolApproval, WebSearch, and OpenTelemetry — so this sample only needs custom instructions
// and a WebBrowsingTool.
// The agent plans research tasks, creates a todo list, gets user approval,
// and then executes each step — all within an interactive conversation loop.
//
// Special commands:
//   /todos  — Display the current todo list without invoking the agent.
//   /mode   — Get or set the current agent mode.
//   /exit   — End the session.

#pragma warning disable OPENAI001 // Suppress experimental API warnings for Responses API usage.
#pragma warning disable MAAI001  // Suppress experimental API warnings for Agents AI experiments.

using DotNetEnv;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

Env.TraversePath().Load();

var azure_endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
    ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is required");
var azure_deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
    ?? "gpt-5-mini";
var azure_api_key = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")
    ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY is required");

var openAIOptions = new OpenAIClientOptions
{
    Endpoint = new Uri(azure_endpoint),
};

AIAgent agent = new OpenAIClient(new ApiKeyCredential(azure_api_key), openAIOptions)
    .GetChatClient(azure_deployment)
    .AsIChatClient()
    .AsHarnessAgent(new HarnessAgentOptions
    {
        Name = "DemoHarnessAgent",
        Description = "A simple HarnessAgent",
        DisableWebSearch = true,
       
        ChatOptions = new ChatOptions
        {
            Instructions = "You are a concise demo assistant. Be helpful and short.",
        },
    });

AgentSession session = await agent.CreateSessionAsync();

Console.WriteLine("Simple HarnessAgent demo");
Console.WriteLine("Type /exit to quit.");

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    if (string.Equals(input, "/exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    var response = await agent.RunAsync(input, session);
    Console.WriteLine(response);
}