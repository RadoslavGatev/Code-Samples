using DotNetEnv;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using System.ComponentModel;

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

await foreach (var update in agent.RunStreamingAsync("Plan me a day trip"))
{
    Console.Write(update);
}

Console.ReadLine();
Console.WriteLine("\n\n=== The chosen destination: ===");
Console.WriteLine(await agent.RunAsync("What's the chosen destination?"));