using Discord;
using Discord.WebSocket;
using PyEnvironment;
using System.Collections.Concurrent;

DiscordSocketConfig config = new()
{
    AlwaysDownloadUsers = true,
    GatewayIntents = GatewayIntents.All,
    MessageCacheSize = 1000
};

DiscordSocketClient _client;

ConcurrentDictionary<ulong, PyInstance> instances = new();

_client = new DiscordSocketClient(config);

await _client.LoginAsync(TokenType.Bot, args[0]);

_client.Log += Log;

_client.MessageReceived += async msg =>
{
    if (msg.Author.IsBot || msg.Channel.Name != "python") return;

    PyInstance node = instances.GetOrAdd(msg.Channel.Id, id => new(
        async output => await msg.Channel.SendMessageAsync(output),
        async error => await msg.Channel.SendMessageAsync($"> {error}")));

    if (msg.Content.StartsWith("```py\n") && msg.Content.EndsWith("\n```"))
        await node.WriteAsync(msg.Content[5..^3]);
};

await _client.StartAsync();

await Task.Delay(-1);

static Task Log(LogMessage msg)
{
    Console.WriteLine(msg.ToString());
    return Task.CompletedTask;
}