namespace Tomoe.Commands.Attributes
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

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
            if (context.Guild == null)
            {
                return true;
            }

            // This forces all commands which use this attribute to edit a response instead of creating it.
            await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new());

            foreach (DiscordInteractionDataOption val in context.Interaction.Data.Options.Where(option => option.Type == ApplicationCommandOptionType.User || (option.Type == ApplicationCommandOptionType.String && option.Value is ulong)))
            {
                DiscordMember discordMember = null;
                if (val.Value is DiscordMember member)
                {
                    discordMember = member;
                }
                else if (val.Value is DiscordUser user)
                {
                    discordMember = await user.Id.GetMember(context.Guild);
                }
                else if (val.Value is ulong id)
                {
                    discordMember = await id.GetMember(context.Guild);
                }

                if (discordMember.Id.GetMember(context.Guild) == null) // Discord allows us to get the member even if they are not in the guild. At the current moment, no moderation commands mess with users who aren't in the guild. As such, we can just ignore this.
                {
                    await context.CreateResponseAsync(new()
                    {
                        Content = $"Error: {discordMember.Mention} is not a member of this guild!"
                    });
                    return false;
                }
                else if (discordMember == context.Member)
                {
                    if (CanSelfPunish)
                    {
                        bool selfPunish = await context.Confirm("Error: You're about to punish yourself. Do you wish to continue?");
                        if (selfPunish)
                        {
                            continue;
                        }
                        else
                        {
                            await context.EditResponseAsync(new()
                            {
                                Content = "Error: Cancelling self punishment."
                            });
                            return false;
                        }
                    }
                    else
                    {
                        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                        {
                            Content = $"Error: Cannot issue self punishment with the {context.CommandName} command.",
                            IsEphemeral = true
                        });
                        return false;
                    }
                }
                else if (!context.Member.HasPermission(RequiredPermissions))
                {
                    // https://stackoverflow.com/a/8949772/10942966, tests if there are more than one flag set in the Permissions enum.
                    if ((RequiredPermissions & (RequiredPermissions - 1)) != 0)
                    {
                        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                        {
                            IsEphemeral = true,
                            Content = $"Error: You're lacking the {RequiredPermissions.ToPermissionString()} permission."
                        });
                    }
                    else
                    {
                        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                        {
                            IsEphemeral = true,
                            Content = "Error: You're lacking following permissions: " + RequiredPermissions.ToPermissionString()
                        });
                    }
                    return false;
                }
                else if (discordMember.Hierarchy >= context.Member.Hierarchy)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Error: {discordMember.Mention}'s highest role is greater than or equal to your highest role. You do not have enough power over them!",
                        IsEphemeral = true
                    });
                    return false;
                }
                else if (discordMember.Hierarchy >= context.Guild.CurrentMember.Hierarchy)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Error: {discordMember.Mention}'s highest role is greater than or equal to my highest role. I do not have enough power over them!",
                        IsEphemeral = true
                    });
                    return false;
                }
                else if (discordMember.IsOwner)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Error: {discordMember.Mention} is the owner!",
                        IsEphemeral = true
                    });
                    return false;
                }
            }
            return true;
        }
    }
}