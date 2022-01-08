using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Tomoe.Commands.Common
{
    public class Support : BaseCommandModule
    {
        private IConfiguration Configuration { get; set; } = null!;

        [Command("support"), Description("Sends the support Discord invite."), Aliases("discord")]
        public async Task SupportAsync(CommandContext context) => await context.RespondAsync(Formatter.EmbedlessUrl(Configuration.GetValue("discord:invite", new Uri("https://discord.gg/Bsv7zSFygc"))));
    }
}