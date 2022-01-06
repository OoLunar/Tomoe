using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Serilog;
using System;
using System.Linq;
using System.Timers;

namespace Tomoe.Utils
{
    public class Update
    {
        private static readonly ILogger _logger = Log.ForContext<Update>();
        public static readonly Timer Timer = new();
        public static readonly string[] Branches = new string[] { "public", "beta" };

        public static void Start()
        {
            if (Program.Config.Update.Branch.ToLowerInvariant() == "none")
            {
                _logger.Information("Not starting auto update timer due to branch being \"none\"");
                return;
            }
#pragma warning disable CS8794
            else if (!Branches.Contains(Program.Config.Update.Branch.ToLowerInvariant()))
            {
                _logger.Information($"Not starting auto update timer due to branch not being recognized. Current branch: {Program.Config.Update.Branch.ToLowerInvariant()}. Available branches: \"public\", \"beta\"");
                return;
            }

            Timer.AutoReset = true;
            Timer.Interval = TimeSpan.FromHours(1).Milliseconds;
            Timer.Elapsed += async (object sender, ElapsedEventArgs ElapsedEventArgs) =>
            {
                string githubLatestVersion = Commands.Moderation.Update.GetLatestVersion().FriendlyName;
                if (githubLatestVersion != Constants.Version)
                {
                    if (Program.Config.Update.AutoUpdate)
                    {
                        Commands.Moderation.Update.Download();
                    }
                    else
                    {
                        _logger.Information($"A new update is available! Latest version: {githubLatestVersion}. Current version: {Constants.Version}.");
                        CommandsNextExtension commandsNext = (await Program.Client.GetCommandsNextAsync()).Values.First();
                        DiscordGuild guild = await commandsNext.Client.GetGuildAsync(Program.Config.Update.GuildId, false);
                        DiscordChannel channel = guild.GetChannel(Program.Config.Update.ChannelId);
                        await channel.SendMessageAsync($"<@{Program.Config.Update.UserId}>: A new update is available! Latest version: {githubLatestVersion}. Current version: {Constants.Version}.\nRun `>>update` to have the bot update!");
                    }
                }
            };
            Timer.BeginInit();
            _logger.Information("Started auto update timer!");
        }
    }
}