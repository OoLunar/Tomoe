using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
    public class Unban : BaseCommandModule
    {
        public Database Database { private get; set; }

        [Command("unban"), RequireGuild, RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers), Aliases("fuck_come_back"), Description("Unbans the victim from the guild, allowing them to rejoin.")]
        public async Task ByUser(CommandContext context, [Description("Who to unban.")] DiscordUser victim, [Description("Why is the vicitm being unbanned."), RemainingText] string unbanReason = Constants.MissingReason) => await Program.SendMessage(context, $"{victim.Mention} has been unbanned{(await Api.Moderation.Unban(context.Client, context.Guild, victim.Id, context.User.Id, context.Message.JumpLink.ToString(), unbanReason) ? '.' : " (Failed to dm).")}");
    }
}