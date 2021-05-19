namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using DSharpPlus.Exceptions;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Commands.Moderation.Attributes;

    public class Ban : BaseCommandModule
    {
        [Command("ban"), RequireGuild, RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers), Aliases("fuck_off"), Description("Permanently bans the victim from the guild, sending them off with a dm."), Punishment(false)]
        public async Task ByUser(CommandContext context, [Description("Who to ban.")] DiscordUser victim, [Description("Why is the victim being banned?"), RemainingText] string banReason = Constants.MissingReason)
        {
            if (await new Punishment(false).ExecuteCheckAsync(context, false))
            {
                if ((await context.Guild.GetBansAsync()).Any(guildUser => guildUser.User.Id == victim.Id))
                {
                    await Program.SendMessage(context, Formatter.Bold($"[Error]: {victim.Mention} is already banned!"));
                    return;
                }

                try
                {
                    await Program.SendMessage(context, $"{victim.Mention} has been banned{(await Api.Moderation.Ban(context.Guild, victim, context.User.Id, context.Message.JumpLink.ToString(), banReason) ? '.' : " (Failed to dm).")}");
                }
                catch (UnauthorizedException)
                {
                    await Program.SendMessage(context, Formatter.Bold($"[Error]: I cannot ban {victim.Mention} due to permissions!"));
                }
            }
        }
    }
}
