using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Configuration;

namespace Tomoe.Commands.Common
{
    public class Support : BaseCommandModule
    {
        public IConfiguration Configuration { private get; init; } = null!;

        [Command("support"), Description("Sends the support Discord invite."), Aliases("discord")]
        public Task SupportAsync(CommandContext context) => context.RespondAsync(Formatter.EmbedlessUrl(Configuration.GetValue("discord:invite", new Uri("https://discord.gg/Bsv7zSFygc"))));
    }
}
