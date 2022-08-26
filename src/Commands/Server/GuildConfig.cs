using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace OoLunar.Tomoe.Commands.Server
{
    [Group("guild_config"), Aliases("config", "gc"), Description("Modifies the bot configuration for the guild.")]
    public sealed class GuildConfig : BaseCommandModule
    {
        [GroupCommand]
        public Task GuildConfigAsync(CommandContext context) => throw new NotImplementedException("Guild Configs ain't happening for a bit, sorry :pray:");
    }
}
