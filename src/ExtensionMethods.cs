namespace Tomoe
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.Exceptions;
    using DSharpPlus.SlashCommands;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Utilities.Types;

    public static class ExtensionMethods
    {
        /// <summary>
        /// Attempts to retrieve the DiscordMember from cache, then the API if the cache does not have the member.
        /// </summary>
        /// <param name="discordGuild">The guild to get the DiscordMember from.</param>
        /// <param name="discordUserId">The id to search for in the DiscordGuild.</param>
        /// <returns>The DiscordMember from the DiscordGuild</returns>
        public static async Task<DiscordMember> GetMember(this ulong discordUserId, DiscordGuild discordGuild)
        {
            try
            {
                return discordGuild.Members.Values.FirstOrDefault(member => member.Id == discordUserId) ?? await discordGuild.GetMemberAsync(discordUserId);
            }
            catch (NotFoundException)
            {
                return null;
            }
            catch (Exception)
            {
                // Exceptions are not our problem
                throw;
            }
        }

        public static async Task<bool> TryDmMember(this DiscordUser discordUser, string message)
        {
            bool sentDm = false;
            if (discordUser != null && !discordUser.IsBot)
            {
                foreach (DiscordClient discordClient in Program.Client.ShardClients.Values)
                {
                    foreach (DiscordGuild discordGuild in discordClient.Guilds.Values)
                    {
                        try
                        {
                            DiscordMember discordMember = await discordGuild.GetMemberAsync(discordUser.Id);
                            await (await discordMember.CreateDmChannelAsync()).SendMessageAsync(message);
                            sentDm = true;
                            break;
                        }
                        catch (Exception) { }
                    }
                }
            }
            return sentDm;
        }

        public static async Task<bool> Confirm(this InteractionContext context, string prompt)
        {
            DiscordButtonComponent yes = new(ButtonStyle.Success, $"{context.InteractionId}-confirm", "Yes", false, new("✅"));
            DiscordButtonComponent no = new(ButtonStyle.Danger, $"{context.InteractionId}-decline", "No", false, new("❌"));
            DiscordComponent[] buttonRow = new[] { yes, no };
            QueueButton queueButton = new(context.InteractionId.ToString(CultureInfo.InvariantCulture), context.User.Id, buttonRow);

            DiscordWebhookBuilder responseBuilder = new();
            responseBuilder.Content = prompt;
            responseBuilder.AddComponents(buttonRow);
            await context.EditResponseAsync(responseBuilder);

            bool confirmed = await queueButton.WaitAsync();

            yes.Disable();
            no.Disable();
            DiscordWebhookBuilder editedResponse = new();
            editedResponse.Content = Constants.Loading + ' ' + prompt;
            editedResponse.AddComponents(buttonRow);
            await context.EditResponseAsync(editedResponse);

            return confirmed;
        }

        public static bool HasPermission(this DiscordMember guildMember, Permissions permission) => !guildMember.Roles.Any() ? guildMember.Guild.EveryoneRole.Permissions.HasPermission(permission) : guildMember.Roles.Any(role => role.Permissions.HasPermission(permission));
    }
}