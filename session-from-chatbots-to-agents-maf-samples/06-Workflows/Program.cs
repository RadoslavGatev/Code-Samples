using DotNetEnv;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;


/// <summary>
/// Creates a translation agent for the specified target language.
/// </summary>
/// <param name="targetLanguage">The target language for translation</param>
/// <param name="chatClient">The chat client to use for the agent</param>
/// <returns>A ChatClientAgent configured for the specified language</returns>
static ChatClientAgent GetTranslationAgent(string targetLanguage, IChatClient chatClient) =>
    new(chatClient, $"You are a translation assistant that translates the provided text to {targetLanguage}.", $"{targetLanguage} agent");


Console.OutputEncoding = System.Text.Encoding.UTF8;
Env.TraversePath().Load();

var azure_endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
    ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is required");
var azure_deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
    ?? "gpt-5-mini";
var azure_api_key = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")
    ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY is required");

var openAIOptions = new OpenAIClientOptions()
{
    Endpoint = new Uri(azure_endpoint)
};

IChatClient chatClient = new OpenAIClient(new ApiKeyCredential(azure_api_key), openAIOptions)
    .GetChatClient(azure_deployment)
    .AsIChatClient();

// Create agents
AIAgent frenchAgent = GetTranslationAgent("French", chatClient);
AIAgent spanishAgent = GetTranslationAgent("Spanish", chatClient);
AIAgent bulgarianAgent = GetTranslationAgent("Bulgarian", chatClient);
AIAgent englishAgent = GetTranslationAgent("English", chatClient);

// Build the workflow by adding executors and connecting them
var workflow = new WorkflowBuilder(frenchAgent)
    .AddEdge(frenchAgent, spanishAgent)
    .AddEdge(spanishAgent, bulgarianAgent)
    .AddEdge(bulgarianAgent, englishAgent)
    .Build();

// Execute the workflow
await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, 
    new ChatMessage(ChatRole.User, "Hello World!"));

// Must send the turn token to trigger the agents.
// The agents are wrapped as executors. When they receive messages,
// they will cache the messages and only start processing when they receive a TurnToken.
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

await foreach (WorkflowEvent evt in run.WatchStreamAsync())
{
    if (evt is AgentResponseUpdateEvent executorComplete)
    {
        Console.WriteLine($"{executorComplete.ExecutorId}: {executorComplete.Data}");
    }
}

Console.WriteLine("\n" + workflow.ToMermaidString());

