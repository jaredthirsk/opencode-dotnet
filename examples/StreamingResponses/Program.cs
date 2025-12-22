// Streaming Responses Example
// Demonstrates subscribing to events and handling streaming responses

using LionFire.OpenCode.Serve;
using LionFire.OpenCode.Serve.Models;

Console.WriteLine("LionFire.OpenCode.Serve - Streaming Responses Example");
Console.WriteLine("======================================================");
Console.WriteLine();

try
{
    await using var client = new OpenCodeClient();
    Console.WriteLine("✓ Connected to OpenCode server");

    var session = await client.CreateSessionAsync();
    Console.WriteLine($"✓ Created session: {session.Id}");
    Console.WriteLine();

    try
    {
        // Method 1: Using non-blocking prompt with event subscription
        Console.WriteLine("Sending prompt (non-blocking)...");
        Console.WriteLine();

        var request = new SendMessageRequest
        {
            Parts = new List<PartInput>
            {
                PartInput.TextInput("Count from 1 to 5, with a brief pause between each number.")
            }
        };

        // Send the prompt without waiting for the full response
        await client.PromptAsyncNonBlocking(session.Id, request);

        Console.WriteLine("Streaming response:");
        Console.Write("> ");

        // Subscribe to events to receive updates
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        await foreach (var evt in client.SubscribeToEventsAsync(cancellationToken: cts.Token))
        {
            // Handle different event types
            switch (evt.Type)
            {
                case "chunk":
                    // Partial content update
                    Console.Write(".");
                    break;

                case "message":
                    // Message complete or updated
                    Console.WriteLine();
                    Console.WriteLine("  [Message event received]");
                    break;

                case "complete":
                    // Response fully complete
                    Console.WriteLine();
                    Console.WriteLine("  [Complete]");
                    break;

                case "error":
                    Console.WriteLine();
                    Console.WriteLine($"  [Error event]");
                    break;
            }

            // Exit when response is complete
            if (evt.Type == "complete" || evt.Type == "error")
            {
                break;
            }
        }

        // Fetch the final message to display the full response
        Console.WriteLine();
        Console.WriteLine("Fetching final response...");

        var messages = await client.ListMessagesAsync(session.Id, limit: 2);
        var assistantMessage = messages.LastOrDefault(m => m.Message?.Role == "assistant");

        if (assistantMessage != null)
        {
            Console.WriteLine();
            Console.WriteLine("Final response:");
            foreach (var part in assistantMessage.Parts ?? new List<Part>())
            {
                if (part.IsTextPart)
                {
                    Console.WriteLine(part.Text);
                }
            }
        }
    }
    finally
    {
        await client.DeleteSessionAsync(session.Id);
        Console.WriteLine();
        Console.WriteLine("✓ Session cleaned up");
    }
}
catch (OpenCodeConnectionException ex)
{
    Console.WriteLine($"❌ Connection Error: {ex.Message}");
    Console.WriteLine("Make sure the OpenCode server is running: opencode serve");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation timed out or was cancelled.");
}
catch (OpenCodeException ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
}

Console.WriteLine();
Console.WriteLine("Done!");
