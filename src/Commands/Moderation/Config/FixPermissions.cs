namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Config : BaseCommandModule
    {
        [Command("fix_permissions"), Aliases("fix_perms"), RequireUserPermissions(Permissions.ManageChannels | Permissions.ManageRoles), Description("Fixes channel permissions for the punishment roles.")]
        public async Task FixPermissions(CommandContext context, [Description("Which punishment role to fix.")] Api.Moderation.RoleAction roleAction, [Description("Which channel to fix. If not set, it'll fix all channels.")] DiscordChannel discordChannel = null)
        {
            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();
            GuildConfig guildConfig = database.GuildConfigs.First(guildConfig => guildConfig.Id == context.Guild.Id);
            ulong currentRole = roleAction switch
            {
                Api.Moderation.RoleAction.Antimeme => guildConfig.AntimemeRole,
                Api.Moderation.RoleAction.Mute => guildConfig.MuteRole,
                Api.Moderation.RoleAction.Voiceban => guildConfig.VoicebanRole,
                Api.Moderation.RoleAction.None or _ => throw new NotImplementedException()
            };
            DiscordRole discordRole = currentRole.GetRole(context.Guild);
            if (discordRole == null)
            {
                // TODO: Prompt to make one for them.
                await Program.SendMessage(context, $"The {roleAction} role has been deleted. Please set another one through `>>config {roleAction} @NewRole`");
                return;
            }

            if (discordChannel == null)
            {
                await FixPermissions(context.Guild, roleAction, discordRole);
                await Program.SendMessage(context, $"Fixed permissions for the {roleAction} role on all channels.");
            }
            else if (discordChannel.Type == ChannelType.Category)
            {
                await FixPermissions(discordChannel, roleAction, discordRole);
                foreach (DiscordChannel channel in discordChannel.Children)
                {
                    await FixPermissions(channel, roleAction, discordRole);
                }
                await Program.SendMessage(context, $"Fixed permissions for the {roleAction} role on category {discordChannel.Mention} and sub channels.");
            }
            else
            {
                await FixPermissions(discordChannel, roleAction, discordRole);
                await Program.SendMessage(context, $"Fixed permissions for the {roleAction} role on channel {discordChannel.Mention}.");
            }
        }
    }
}
