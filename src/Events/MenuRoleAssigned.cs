using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Attributes;
using Tomoe.Models;

namespace Tomoe.Events
{
    public class MenuRoleAssigned
    {
        [SubscribeToEvent(nameof(DiscordShardedClient.ComponentInteractionCreated))]
        public static async Task HandleButtonPress(DiscordClient client, ComponentInteractionCreateEventArgs componentInteractionCreateEventArgs)
        {
            if (!componentInteractionCreateEventArgs.Id.StartsWith("menurole\v", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }
            await componentInteractionCreateEventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true));

            string[] idParts = componentInteractionCreateEventArgs.Id.Split('\v');
            if (!componentInteractionCreateEventArgs.Guild.Members.TryGetValue(componentInteractionCreateEventArgs.User.Id, out DiscordMember? member))
            {
                member = await componentInteractionCreateEventArgs.Guild.GetMemberAsync(componentInteractionCreateEventArgs.User.Id);
            }
            DatabaseContext? database = Program.ServiceProvider.GetService<DatabaseContext>();

            if (database == null)
            {
                await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("[Error]: Internal bot error, your vote was not casted. Please try again later."));
                throw new InvalidOperationException("DatabaseContext is null!");
            }
            else if (idParts.Length != 3)
            {
                await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("[Error]: Internal bot error, your vote was not casted. Please try again later."));
                throw new InvalidOperationException("Invalid poll id!");
            }
            else if (!Guid.TryParse(idParts[1], out Guid pollId))
            {
                await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("[Error]: Internal bot error, your vote was not casted. Please try again later."));
                throw new InvalidOperationException("Invalid poll id!");
            }

            IEnumerable<DiscordRole> menuRoles = database.MenuRoles.Where(x => x.ButtonId == idParts[1] && x.GuildId == componentInteractionCreateEventArgs.Guild.Id).AsEnumerable().Select(x => componentInteractionCreateEventArgs.Guild.GetRole(x.RoleId)).OrderByDescending(x => x.Position);
            IEnumerable<DiscordRole> memberMenuRoles = member.Roles.Intersect(menuRoles);
            if (string.Equals(idParts[2], "select", StringComparison.OrdinalIgnoreCase))
            {
                List<DiscordSelectComponentOption> options = new()
                {
                    new DiscordSelectComponentOption("No Roles", "0", "Removes all roles from you.", false, new DiscordComponentEmoji("❌"))
                };
                foreach (DiscordRole role in memberMenuRoles)
                {
                    options.Add(new DiscordSelectComponentOption(role.Name, role.Id.ToString(CultureInfo.InvariantCulture), $"Removes the \"{role.Name}\" role from you.", true, new DiscordComponentEmoji("✅")));
                }
                foreach (DiscordRole role in menuRoles.Except(memberMenuRoles))
                {
                    options.Add(new DiscordSelectComponentOption(role.Name, role.Id.ToString(CultureInfo.InvariantCulture), $"Assigns the \"{role.Name}\" role to you.", false, new DiscordComponentEmoji("❓")));
                }

                await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Select whichever roles you want.").AddComponents(new DiscordSelectComponent($"menurole\v{idParts[1]}\vassign", "Select your roles!", options, false, 0, options.Count)));
                return;
            }
            else if (string.Equals(idParts[2], "assign", StringComparison.OrdinalIgnoreCase))
            {
                if (componentInteractionCreateEventArgs.Values.Contains("0")) // Keeping it as a string since that's how Discord gives the values to us.
                {
                    await member.ModifyAsync(x => x.Roles = member.Roles.Except(menuRoles).ToList());
                    await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("All menu roles have been removed since you chose the \"No Roles\" option."));
                    return;
                }

                IEnumerable<DiscordRole> grantRoles = menuRoles.Where(x => componentInteractionCreateEventArgs.Values.Contains(x.Id.ToString(CultureInfo.InvariantCulture)));
                IEnumerable<DiscordRole> revokeRoles = menuRoles.Except(grantRoles);
                await member.ModifyAsync(x => x.Roles = member.Roles.Concat(grantRoles).Except(revokeRoles).ToList());
                await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Thanks {componentInteractionCreateEventArgs.User.Mention}. Your menu roles have been updated accordingly."));
            }
        }
    }
}
