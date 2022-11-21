using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Humanizer;

namespace Tomoe.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class HierarchyAttribute : SlashCheckBaseAttribute
    {
        private readonly bool CanSelfPunish;
        private readonly Permissions RequiredPermissions;

        /// <summary>
        /// Checks if the user is running a moderation command on themself, causing a "self punishment." Additionally checks to see if they have the required permissions if it is not a self punishment.
        /// </summary>
        /// <param name="requiredPermissions">The Discord Permissions required for the user.</param>
        /// <param name="canSelfPunish">Can the user execute the command on themself?</param>
        /// <remarks>Instead of calling context.CreateInteractionResponseAsync, you need to call EditResponseAsync.</remarks>
        public HierarchyAttribute(Permissions requiredPermissions = Permissions.None, bool canSelfPunish = false)
        {
            CanSelfPunish = canSelfPunish;
            RequiredPermissions = requiredPermissions;
        }

        public override async Task<bool> ExecuteChecksAsync(InteractionContext context)
        {
            // Hierarchies are only checked in guilds.
            if (context.Guild is null)
            {
                return true;
            }
            else if (!context.Member.Permissions.HasPermission(RequiredPermissions))
            {
                // https://stackoverflow.com/a/8949772/10942966, test if we're missing a singular permission or multiple permissions (for the error message).
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    IsEphemeral = true,
                    Content = (RequiredPermissions & (RequiredPermissions - 1)) != 0 // Has more than 1 flag
                        ? "Error: You're lacking some of the following permissions: " + RequiredPermissions.Humanize(LetterCasing.LowerCase)
                        : $"Error: You're lacking the {RequiredPermissions.Humanize(LetterCasing.LowerCase)} permission."
                });
                return false;
            }

            // This forces all commands which use this attribute to edit a response instead of creating it.
            await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new());

            // Iterate through all the users mentioned in the command.
            foreach (DiscordInteractionDataOption value in context.Interaction.Data.Options)
            {
                DiscordMember? member = null;

                // Check if the option is a user id.
                if (value.Type == ApplicationCommandOptionType.User)
                {
                    member = await context.Guild.GetMemberAsync((ulong)value.Value);
                }
                // Check if the option is a snowflake (which must be passed through a string)
                else if (value.Type == ApplicationCommandOptionType.String && ulong.TryParse((string)value.Value, out ulong userId))
                {
                    member = await context.Guild.GetMemberAsync(userId);
                }

                // If value.Type is not a user or a string, is a user but isn't in the guild, or is a string but isn't a valid user ID, skip.
                if (member is null)
                {
                    continue;
                }

                // Check to see if the user can use the command on themself.
                if (member == context.Member)
                {
                    if (CanSelfPunish)
                    {
                        bool confirmed = await context.ConfirmAsync($"Error: You're about to use `/{context.CommandName}` yourself. Do you wish to continue?");
                        if (confirmed)
                        {
                            continue;
                        }
                        else
                        {
                            await context.EditResponseAsync(new()
                            {
                                Content = $"Error: Cancelling `/{context.CommandName}`."
                            });
                            return false;
                        }
                    }
                    else
                    {
                        await context.EditResponseAsync(new()
                        {
                            Content = $"Error: `/{context.CommandName}` does not allow itself to be used on the command invoker."
                        });
                        return false;
                    }
                }
                else if (member.Hierarchy >= context.Member.Hierarchy)
                {
                    await context.EditResponseAsync(new()
                    {
                        Content = $"Error: {member.Mention}'s highest role is greater than or equal to your highest role. You do not have enough power over them!"
                    });
                    return false;
                }
                else if (member.Hierarchy >= context.Guild.CurrentMember.Hierarchy)
                {
                    await context.EditResponseAsync(new()
                    {
                        Content = $"Error: {member.Mention}'s highest role is greater than or equal to my highest role. I do not have enough power over them!"
                    });
                    return false;
                }
            }
            return true;
        }
    }
}
