# Discord Channel Image Script

This tool allows you to download most of images on a single channel.

Maximum images is 1000 but you can configure it later.

## Prerequisites:
.NET 10 SDK or later.

## Usage:
1. Create a Discord bot and put your token under `.env` file:
```bash
DISCORD_BOT_TOKEN=<your_token_here>
```
2. Invite your discord bot in your own server.
3. Run the DiscordApp project via `dotnet run -c Release`.
4. Run `/fetch <channel>` command on Discord. This should output a json that contains all images urls.

### From FetchImages:
1. Run FetchImages by using `dotnet ./FetchImages.cs -i <path/to/jsonfile>`
2. Wait for it to be completed.
3. Done!

## Frequently Asked Question:
Q: Why not download the images directly?
A: Because of Discord ratelimiter that may stop the command.

Q: May I use other .NET versions?
A: Yes, but you may need to create a new project for FetchImages, since single-file application is only available on
.NET 10