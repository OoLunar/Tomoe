namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using System;
    using System.Threading.Tasks;
    using Tomoe.Commands.Moderation.Attributes;
    using Tomoe.Utils.Exceptions;
    using static Tomoe.Commands.Moderation.ModLogs;

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
                await Program.SendMessage(context, $"{victim.Mention} has been kicked{(await ByProgram(context.Guild, guildVictim, context.User, context.Message.JumpLink, kickReason) ? '.' : " (Failed to dm).")}");
            }
        }

        public static async Task<bool> ByProgram(DiscordGuild discordGuild, DiscordMember victim, DiscordUser issuer, Uri jumplink, [RemainingText] string kickReason = Constants.MissingReason)
        {
            if (victim.Hierarchy >= (await issuer.Id.GetMember(discordGuild)).Hierarchy)
            {
                throw new HierarchyException();
            }

            bool sentDm = await victim.TryDmMember($"You've been kicked from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(kickReason))}Context: {jumplink}");
            await victim.RemoveAsync(kickReason);
            await Record(discordGuild, LogType.Kick, null, $"{issuer.Mention} kicked {victim.Mention}{(sentDm ? '.' : " (Failed to dm).")} Reason: {kickReason}");
            return sentDm;
        }
    }
}
