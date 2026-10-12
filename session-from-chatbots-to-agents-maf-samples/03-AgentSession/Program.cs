using DotNetEnv;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using System.ComponentModel;
using System.Text.Json;

[Description("Provides a random vacation destination.")]
static string GetRandomDestination()
{
    var destinations = new List<string>
    {
        "Paris, France",
        "Tokyo, Japan",
        "New York City, USA",
        "Sydney, Australia",
        "Rome, Italy",
        "Barcelona, Spain",
        "Cape Town, South Africa",
        "Rio de Janeiro, Brazil",
        "Bangkok, Thailand",
        "Vancouver, Canada"
    };

    var random = new Random();
    int index = random.Next(destinations.Count);
    return destinations[index];
}

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

AIAgent agent = new OpenAIClient(new ApiKeyCredential(azure_api_key), openAIOptions)
    .GetChatClient(azure_deployment)
    .AsIChatClient()
    .AsAIAgent(
        name: "TravelAgent",
        instructions: "You are a helpful AI Agent that can help plan vacations for customers at random destinations.",
        tools: [AIFunctionFactory.Create((Func<string>)GetRandomDestination)]
    );

// Create a session to maintain conversation history
AgentSession session = await agent.CreateSessionAsync();

await foreach (var update in agent.RunStreamingAsync("Plan me a day trip", session))
{
    Console.Write(update);
}

Console.ReadLine();
Console.WriteLine("\n\n=== The chosen destination: ===");
Console.WriteLine(await agent.RunAsync("What's the chosen destination?", session));


Console.ReadLine();
// Serialize the session state to a JsonElement, so it can be stored for later use.
JsonElement serializedSession = await agent.SerializeSessionAsync(session);
// In a real application, you would typically write the serialized session to a file or
// database for persistence, and read it back when resuming the conversation.
// Here we'll just write the serialized session to console (for demonstration purposes).
Console.WriteLine("\n\n=== Serialized: ===");
Console.WriteLine(JsonSerializer.Serialize(serializedSession, new JsonSerializerOptions { WriteIndented = true }) + "\n");


Console.ReadLine();
// Deserialize the session state after loading from storage.
AgentSession resumedSession = await agent.DeserializeSessionAsync(serializedSession);

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("\n\n=== Resumed session: ===");
// Run the agent again with the resumed session.
Console.WriteLine(await agent.RunAsync("Now give me thе existing day trip plan, and add some emojis to it.", resumedSession));
