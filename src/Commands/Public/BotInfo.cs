namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class BotInfo : SlashCommandModule
    {
        [SlashCommand("bot_info", "Gets general info about the bot.")]
        public async Task Overload(InteractionContext context)
        {
            await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new()
            {
                IsEphemeral = true
            });

            StringBuilder botInfo = new();
            botInfo.Append($"Currently in {context.Client.Guilds.Count} guilds\n");
            botInfo.Append($"Handling around {Listeners.GuildDownloadCompleted.MemberCount.Values.Sum().ToMetric()} guild members\n");
            botInfo.Append($"Websocket Ping: {context.Client.Ping}ms\n");
            //botInfo.Append($"Total shards: {Program.Client.ShardClients.Count}\n");
            Process currentProcess = Process.GetCurrentProcess();
            botInfo.Append($"Total memory used: {Math.Round(currentProcess.PrivateMemorySize64.Bytes().Megabytes, 2).ToMetric()}mb\n");
            botInfo.Append($"Total threads open: {currentProcess.Threads.Count}");
            currentProcess.Dispose();

            DiscordEmbedBuilder embedBuilder = new();
            embedBuilder.Title = "Bot Info";
            embedBuilder.Color = new DiscordColor("#7b84d1");
            embedBuilder.Description = botInfo.ToString();
            if (context.Guild != null && context.Guild.IconUrl != null)
            {
                embedBuilder.WithThumbnail(context.Guild.IconUrl);
            }

            DiscordWebhookBuilder webhookBuilder = new();
            webhookBuilder.AddEmbed(embedBuilder);
            await context.EditResponseAsync(webhookBuilder);
        }
    }
}
