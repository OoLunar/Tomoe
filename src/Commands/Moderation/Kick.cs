namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using DSharpPlus.Exceptions;
    using System.Threading.Tasks;
    using Tomoe.Commands.Moderation.Attributes;

    public class Kick : BaseCommandModule
    {
        [Command("kick"), RequireGuild, RequireUserPermissions(Permissions.KickMembers), RequireBotPermissions(Permissions.KickMembers), Aliases("boot", "yeet"), Description("Kicks the victim from the guild, sending them off with a dm."), Punishment(true)]
        public async Task ByUser(CommandContext context, [Description("Who to remove from the guild.")] DiscordUser victim, [Description("Why is the victim being removed from the guild?"), RemainingText] string kickReason = Constants.MissingReason)
        {
            DiscordMember guildVictim = await victim.Id.GetMember(context.Guild);
            if (guildVictim == null)
            {
                await Program.SendMessage(context, Formatter.Bold($"[Error]: User {victim.Mention} is not in the guild!"));
            }
            else
            {
                try
                {
                    await Program.SendMessage(context, $"{victim.Mention} has been kicked{(await Api.Moderation.Kick(context.Guild, guildVictim, context.User.Id, context.Message.JumpLink.ToString(), kickReason) ? '.' : " (Failed to dm).")}");

                }
                catch (UnauthorizedException)
                {
                    await Program.SendMessage(context, Formatter.Bold($"[Error]: I cannot kick {victim.Mention} due to permissions!"));
                }
            }
        }
    }
}
