using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using OoLunar.Tomoe.Configuration;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// It can't support me... I'm too heavy.
    /// </summary>
    public sealed class SupportCommand
    {
        private readonly string? _supportServerInvite;

        /// <summary>
        /// Creates a new instance of <see cref="SupportCommand"/>.
        /// </summary>
        /// <param name="tomoeConfiguration">Required service for accessing the support invite.</param>
        public SupportCommand(TomoeConfiguration tomoeConfiguration) => _supportServerInvite = tomoeConfiguration.Discord.SupportInvite;

        /// <summary>
        /// Sends an invite to my Discord server. You can receive support there.
        /// </summary>
        [Command("support"), TextAlias("server")]
        public ValueTask ExecuteAsync(CommandContext context) => context.RespondAsync(_supportServerInvite is null
            ? "I'm sorry, but I don't seem to have a support server."
            : $"Are you looking for support? You can join my support server here: <{_supportServerInvite}>");
    }
}
