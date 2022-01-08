using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Tomoe.Commands.Common
{
    public class Repository : BaseCommandModule
    {
        private IConfiguration Configuration { get; set; } = null!;

        [Command("repository"), Description("Sends the source code for Tomoe."), Aliases("github", "gh", "gitlab", "repo")]
        public async Task Overload(CommandContext context) => await context.RespondAsync(Formatter.EmbedlessUrl(Configuration.GetValue("repository_link", new Uri("https://github.com/OoLunar/Tomoe"))));
    }
}