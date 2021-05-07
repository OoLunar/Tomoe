namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using System;
    using System.Threading.Tasks;
    using Tomoe.Commands.Moderation.Attributes;
    using Tomoe.Db;
    using static Tomoe.Commands.Moderation.ModLogs;

    public class Unban : BaseCommandModule
    {
        public Database Database { private get; set; }

        [Command("unban"), RequireGuild, RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers), Aliases("fuck_come_back"), Description("Unbans the victim from the guild, allowing them to rejoin."), Punishment(false)]
        public async Task ByUser(CommandContext context, [Description("Who to unban.")] DiscordUser victim, [Description("Why is the vicitm being unbanned."), RemainingText] string unbanReason = Constants.MissingReason)
        {
            bool sentDm = await ByProgram(context.Guild, victim, context.User, context.Message.JumpLink, unbanReason);
            _ = await Program.SendMessage(context, $"{victim.Mention} has been unbanned{(sentDm ? '.' : " (Failed to dm).")}");
        }

        public static async Task<bool> ByProgram(DiscordGuild discordGuild, DiscordUser victim, DiscordUser issuer, Uri jumplink, [RemainingText] string unbanReason = Constants.MissingReason)
        {
            await discordGuild.UnbanMemberAsync(victim.Id, unbanReason);
            bool sentDm = await (await victim.Id.GetMember(discordGuild)).TryDmMember($"You've been unbanned from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(unbanReason))}Context: {jumplink}");
            await Record(discordGuild, LogType.Unban, null, $"{issuer.Mention} unbanned {victim.Mention}{(sentDm ? '.' : " (Failed to dm).")} Reason: {unbanReason}");
            return sentDm;
        }
    }
}
