using System;
using System.IO;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using EdgeDB.State;
using Microsoft.Extensions.Configuration;
using OoLunar.Tomoe;

namespace Tomoe.Test
{
    [TestClass]
    public sealed class CommandTester
    {
        private static readonly Program Program = new();
        private static readonly IConfiguration Configuration;
        private static readonly CommandContext Context;

        static CommandTester()
        {
            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.Sources.Clear();

            // Load the default configuration from the config.json file
            string configurationFilePath = Path.Join(Environment.CurrentDirectory, "res", "config.json");
            if (File.Exists(configurationFilePath))
            {
                configurationBuilder.AddJsonFile(Path.Join(Environment.CurrentDirectory, "res", "config.json"), true, true);
            }

            // Override the default configuration with the environment variables
            configurationBuilder.AddEnvironmentVariables("DISCORD_BOT_");

            Configuration = configurationBuilder.Build();
            _ = Program.Main(Array.Empty<string>());

            DiscordClient client = new(new DiscordConfiguration
            {
                Token = Configuration.GetValue<string>("token"),
            });

            client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { Configuration.GetValue<string>("prefix") },
            });

            client.ConnectAsync().GetAwaiter().GetResult();

            DiscordUser user = client.GetUserAsync(Configuration.GetValue<ulong>("testing:user_id")).GetAwaiter().GetResult();
            DiscordGuild guild = client.GetGuildAsync(Configuration.GetValue<ulong>("testing:guild_id")).GetAwaiter().GetResult();
            DiscordChannel channel = guild.GetChannel(Configuration.GetValue<ulong>("testing:channel_id"));
            DiscordMessage message = channel.SendMessageAsync(Configuration.GetValue<string>("testing:message")).GetAwaiter().GetResult();

            client.GetCommandsNext().CreateFakeContext()
        }

        [TestMethod]
        public void PublicCommands()
        {
        }
    }
}
