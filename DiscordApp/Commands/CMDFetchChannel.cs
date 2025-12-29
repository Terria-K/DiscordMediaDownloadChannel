using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Newtonsoft.Json;

namespace DiscordApp.Commands;

public class CMDFetchChannel : InteractionModuleBase<SocketInteractionContext>
{
    // public List<ImageData> images;
    public List<string> Urls;


    [SlashCommand("fetch", "Fetch all images from a channel")]
    public async Task HandleCommand(
        [Summary("channel", "The channel you want to fetch")] ITextChannel channel
    )
    {
        await DeferAsync(true);
        Urls = new List<string>();
        // images = new ();

        var latestMessage = (await channel.GetMessagesAsync(1).FlattenAsync()).FirstOrDefault();
        ulong? lastMessageID = latestMessage?.Id;

        var collectedMedia = new List<IMessage>();

        while (collectedMedia.Count < Constants.MAXIMUM_MESSAGES)
        {
            var batches = await channel.GetMessagesAsync(
                lastMessageID ?? ulong.MaxValue,
                Direction.Before,
                100
            ).FlattenAsync();

            if (!batches.Any())
            {
                break;
            }

            var mediaBatches = batches.Where(x => x.Attachments.Count > 0);
            collectedMedia.AddRange(mediaBatches);
            lastMessageID = batches.Min(m => m.Id);

            if (collectedMedia.Count >= Constants.MAXIMUM_MESSAGES)
            {
                break;
            }

            await ModifyOriginalResponseAsync(msg =>
            {
                msg.Content = $"Collected: {collectedMedia.Count}";
            });
        }


        if (collectedMedia.Count == 0)
        {
            await FollowupAsync("No messages with attachments found.", ephemeral: true);
            return;
        }


        foreach (var mess in collectedMedia) 
        {
            ReadAttachment(mess.Attachments);
        }

        var str = JsonConvert.SerializeObject(Urls, Formatting.Indented);
        using var fs = new FileStream(Path.Combine("Result", $"{channel.Name}.json"), FileMode.Create, FileAccess.Write);
        using TextWriter tx = new StreamWriter(fs);
        tx.Write(str);

        await FollowupAsync("DONE!", ephemeral: true);
    }

    private void ReadAttachment(IReadOnlyCollection<IAttachment> attachments) 
    {
        foreach (var attach in attachments) 
        {
            Urls.Add(attach.Url);
        }
    }
}
