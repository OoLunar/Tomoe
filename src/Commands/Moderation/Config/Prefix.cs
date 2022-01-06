using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tomoe.Commands.Moderation
{
    public partial class Config : BaseCommandModule
    {
        [Command("prefixes"), Description("Shows currently allowed prefixes.")]
        public async Task Prefixes(CommandContext context) => await Program.SendMessage(context, $"Prefixes => {string.Join(", ", ((List<string>)Api.Moderation.Config.Get(context.Guild.Id, Api.Moderation.Config.ConfigSetting.GuildPrefixes)).Select(prefix => Formatter.Sanitize(prefix)).DefaultIfEmpty("None set"))}.");

        [Command("prefix_add"), RequireUserPermissions(Permissions.ManageGuild), Description("Adds a prefix that the bot responds to.")]
        public async Task PrefixAdd(CommandContext context, string prefix)
        {
            await Api.Moderation.Config.AddList(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.GuildPrefixes, prefix);
            await Program.SendMessage(context, $"Added \"{prefix}\" as a prefix!");
        }

        [Command("prefix_remove"), RequireUserPermissions(Permissions.ManageGuild), Description("Removes a prefix that the bot responds to.")]
        public async Task PrefixRemove(CommandContext context, string prefix)
        {
            if (await Api.Moderation.Config.RemoveList(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.GuildPrefixes, prefix))
            {
                await Program.SendMessage(context, $"Removed the \"{prefix}\" prefix!");
            }
            else
            {
                await Program.SendMessage(context, $"\"{prefix}\" was never a prefix!");
            }
        }

        [Command("prefixes_clear"), Aliases("prefix_clear"), RequireUserPermissions(Permissions.ManageGuild), Description("Clears all guild prefixes from the prefix list.")]
        public async Task PrefixClear(CommandContext context)
        {
            await Api.Moderation.Config.ClearList(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.GuildPrefixes);
            await Program.SendMessage(context, "All prefixes have been cleared from the prefix list!");
        }
    }
}