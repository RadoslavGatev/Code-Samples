using GitHub.Copilot;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.GitHub.Copilot;

SessionConfig sessionConfig = new()
{
    OnPermissionRequest = PermissionHandler.ApproveAll
};

await using var copilotClient = new CopilotClient();
AIAgent agent = copilotClient.AsAIAgent(sessionConfig, ownsClient: true);
AgentSession agentSession = await agent.CreateSessionAsync();

var exitRequested = false;
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    exitRequested = true;
};

Console.WriteLine("Chat with Copilot Agent Framework demo.");
Console.WriteLine("Type a prompt, or `/exit` to quit.\n");

while (!exitRequested)
{
    Console.Write("You: ");
    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    if (string.Equals(input, "/exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    Console.Write("Assistant: ");
    await foreach (var update in agent.RunStreamingAsync(input, agentSession))
    {
        Console.Write(update);
    }

    Console.WriteLine("\n");
}

Console.WriteLine("Session ended.");
