namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using System.Threading.Tasks;

    public class Repository : BaseCommandModule
    {
        [Command("repository"), Description("Sends the source code for Tomoe."), Aliases("github", "gh", "gitlab", "repo")]
        public async Task Overload(CommandContext context) => await Program.SendMessage(context, Formatter.EmbedlessUrl(new(Program.Config.RepositoryLink)));

    }
}
