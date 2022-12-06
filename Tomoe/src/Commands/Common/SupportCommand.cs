using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class SupportCommand : BaseCommand
    {
        private readonly string? _supportServerInvite;

        public SupportCommand(IConfiguration configuration) => _supportServerInvite = configuration.GetValue<string>("discord:debug_guild_id");

        [Command("support", "server")]
        public Task ExecuteAsync(CommandContext context) => context.ReplyAsync(_supportServerInvite is null
            ? "I'm sorry, but the owner of the bot doesn't seem to have setup a support server."
            : $"Are you looking for support? You can join my support server here: <{_supportServerInvite}>");
    }
}
