using System.Threading.Tasks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;
using Microsoft.Extensions.Configuration;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class SupportCommand(IConfiguration configuration)
    {
        private readonly string? _supportServerInvite = configuration.GetValue<string>("discord:debug_guild_id");

        [Command("support"), TextAlias("server")]
        public async Task ExecuteAsync(CommandContext context) => await context.RespondAsync(_supportServerInvite is null
            ? "I'm sorry, but the owner of the bot doesn't seem to have setup a support server."
            : $"Are you looking for support? You can join my support server here: <{_supportServerInvite}>");
    }
}
