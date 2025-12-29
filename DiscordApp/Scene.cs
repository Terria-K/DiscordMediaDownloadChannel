using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using DiscordApp.Commands;
using dotenv.net;
using System.IO;
using Discord.Interactions;

namespace DiscordApp;

public class Scene
{
    private DiscordSocketClient client;

    public async Task MainAsync()
    {
        if (!Directory.Exists("Result"))
        {
            Directory.CreateDirectory("Result");
        }
        DotEnv.Load();

        var env = DotEnv.Read();
        var config = new DiscordSocketConfig { MessageCacheSize = 100 };

        client = new DiscordSocketClient(config);
        client.Log += Log;

        await client.LoginAsync(TokenType.Bot, env["DISCORD_BOT_TOKEN"]);
        await client.StartAsync();


        client.Ready += OnReady;

        await Task.Delay(-1);
    }

    private async Task OnInteraction(InteractionService commands, SocketInteraction interaction)
    {
        var ctx = new SocketInteractionContext(client, interaction);
        var result = await commands.ExecuteCommandAsync(ctx, null);

        if (!result.IsSuccess)
        {
            Console.WriteLine($"Command failed: {result.Error} - {result.ErrorReason}");
        }

        await Task.CompletedTask;
    }

    private async Task OnReady()
    {
        var env = DotEnv.Read();
        Console.WriteLine("Bot is connected!");

        var commands = new InteractionService(client);

        await commands.AddModulesAsync(typeof(CMDFetchChannel).Assembly, null);
        await commands.RegisterCommandsToGuildAsync(ulong.Parse(env["SERVER_ID"]));

        client.InteractionCreated += async (ctx) => await OnInteraction(commands, ctx);

        // try
        // {
        //     for (int i = 0; i < commands.Count; i++)
        //     {
        //         await client.Rest.CreateGuildCommand(commands[i].BuildSlashCommand(), GuildID);
        //     }
        // }
        // catch (HttpException ex)
        // {
        //     var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
        //     Console.WriteLine(json);
        // }
        await Task.CompletedTask;
    }

    // private async Task SlashCommandHandler(SocketSlashCommand command)
    // {
    //     var target = commands
    //         .Where(x => command.Data.Name == x.CommandName)
    //         .Select(x => x)
    //         .First();

    //     if (target is not null)
    //     {
    //         await target.HandleCommand(command);
    //     }
    // }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}

