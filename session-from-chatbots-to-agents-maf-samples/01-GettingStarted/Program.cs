using DotNetEnv;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

Env.TraversePath().Load();

// Get Azure OpenAI configuration from environment variables
var azure_endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
    ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is required");
var azure_deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
    ?? "gpt-5-mini";
var azure_api_key = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")
    ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY is required");

// Configure OpenAI client for Azure OpenAI
var openAIOptions = new OpenAIClientOptions()
{
    Endpoint = new Uri(azure_endpoint)
};

// Create AI Agent with custom tool
AIAgent agent = new OpenAIClient(new ApiKeyCredential(azure_api_key), openAIOptions)
    .GetChatClient(azure_deployment)
    .AsIChatClient()
    .AsAIAgent(
        name: "TravelAgent",
        instructions: "You are a helpful AI Agent that can help plan vacations for customers at random destinations."
    );

// Run agent with standard response
Console.WriteLine("=== Travel Plan ===");
Console.WriteLine(await agent.RunAsync("Plan me a day trip"));
Console.ReadLine();

// Run agent with streaming response
Console.WriteLine("\n=== Streaming Travel Plan ===");
await foreach (var update in agent.RunStreamingAsync("Plan me a day trip"))
{
    Console.Write(update);
}
Console.WriteLine();
