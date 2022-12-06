using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;
using OoLunar.DSharpPlus.CommandAll.Converters;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Database.Models;
using OoLunar.Tomoe.Events;
using OoLunar.Tomoe.Services;

namespace OoLunar.Tomoe.Commands.Server
{
    [Command("role_menu")]
    public sealed class RoleMenuCommand : BaseCommand
    {
        private static readonly DiscordChannelArgumentConverter _channelConverter = new();
        private static readonly DiscordMessageArgumentConverter _messageConverter = new();
        private static readonly DiscordEmojiArgumentConverter _emojiConverter = new();
        private static readonly TextInputComponent[] _prompts = new[] {
            new TextInputComponent("What should the role menu message say?", "1", "Come grab your roles!", null, true, TextInputStyle.Paragraph, 1, 2000),
            new TextInputComponent("What about the text on the button?", "2", "Click here to get your roles!", null, true, TextInputStyle.Short, 1, 80),
            new TextInputComponent("Which emoji should the button have?", "3", "⚙️", null, false, TextInputStyle.Short, 1),
            new TextInputComponent("Which channel should this be posted in?", "4", "general", null, false, TextInputStyle.Short, 1, 100),
            new TextInputComponent("Should this add onto an existing message?", "5", "https://discord.com/channels/832354798153236510/832374606748188743/1049080304545058896", null, false, TextInputStyle.Short, 1)
        };

        private readonly IServiceProvider _serviceProvider;
        private readonly RoleMenuService _roleMenuService;

        public RoleMenuCommand(IServiceProvider serviceProvider, RoleMenuService roleMenuService)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _roleMenuService = roleMenuService ?? throw new ArgumentNullException(nameof(roleMenuService));
        }

        [Command("create")]
        public async Task CreateAsync(CommandContext context, params DiscordRole[] roles)
        {
            IReadOnlyList<string>? responses = await context.PromptAsync(_prompts);
            if (responses is null)
            {
                await context.ReplyAsync($"The role creation process timed out! Remember that you have to respond within {Formatter.Timestamp(context.PromptTimeout, TimestampFormat.RelativeTime)}.");
                return;
            }

            Optional<DiscordChannel> channelOptional = await _channelConverter.ConvertAsync(context, null!, responses[3]);
            if (channelOptional.IsDefined(out DiscordChannel? channel))
            {
                if (!channel!.PermissionsFor(context.Member).HasPermission(Permissions.SendMessages))
                {
                    await context.ReplyAsync($"You do not have permission to send messages in {channel.Mention}.");
                    return;
                }
                else if (!channel!.PermissionsFor(context.Guild!.CurrentMember).HasPermission(Permissions.SendMessages))
                {
                    await context.ReplyAsync($"I do not have permission to send messages in {channel.Mention}.");
                    return;
                }
            }

            Optional<DiscordEmoji> emojiOptional = default;
            if (!string.IsNullOrWhiteSpace(responses[2]))
            {
                emojiOptional = await _emojiConverter.ConvertAsync(context, null!, responses[2]);
            }

            RoleMenuModel roleMenu = _roleMenuService.AddRoleMenu(context.Guild!.Id, roles.Select(role => role.Id).ToArray());
            DiscordButtonComponent button = new(ButtonStyle.Primary, $"{roleMenu.Id}:list", responses[1], false, emojiOptional.IsDefined(out DiscordEmoji? emoji) ? new(emoji) : null);

            DiscordMessageBuilder messageBuilder = new() { Content = responses[0] };
            Optional<DiscordMessage> messageOptional = await _messageConverter.ConvertAsync(context, null!, responses[4]);
            if (messageOptional.IsDefined(out DiscordMessage? message))
            {
                if (message!.Author != context.Guild!.CurrentMember)
                {
                    await context.ReplyAsync($"I cannot edit message <{message.JumpLink}>.");
                    return;
                }

                DiscordActionRowComponent actionRowComponent = message.Components.First();
                if (actionRowComponent.Components.Count == 5)
                {
                    await context.ReplyAsync($"Message <{message.JumpLink}> already has 5 role menus on it. Try creating a new message.");
                    return;
                }

                messageBuilder.AddComponents(actionRowComponent.Components.Append(button));
                await message!.ModifyAsync(messageBuilder);
            }
            else
            {
                messageBuilder.AddComponents(button);
                await channel!.SendMessageAsync(messageBuilder);
            }

            await context.ReplyAsync("Role menu created!");
        }

        [Command("edit")]
        public async Task EditAsync(CommandContext context, DiscordMessage message, params DiscordRole[] roles)
        {
            if (message.Author != context.Guild!.CurrentMember)
            {
                await context.ReplyAsync($"I cannot edit message <{message.JumpLink}>.");
                return;
            }
            else if (message.Components.Count == 0)
            {
                await context.ReplyAsync($"Message <{message.JumpLink}> is not a role menu!");
                return;
            }

            DiscordActionRowComponent? actionRowComponent = message.Components.FirstOrDefault();
            if (actionRowComponent is null)
            {
                await context.ReplyAsync($"Message <{message.JumpLink}> is not a role menu!");
                return;
            }

            int editRoleMenu = 0;
            if (actionRowComponent.Components.Count > 1)
            {
                IReadOnlyList<string>? responses = await context.PromptAsync(new TextInputComponent("Which role menu would you like to edit?", "1", $"Any number between 1 and {actionRowComponent.Components.Count}", null, true, TextInputStyle.Short, 1, 1));
                if (responses is null)
                {
                    await context.ReplyAsync($"The role menu edit process timed out! Remember that you have to respond within {Formatter.Timestamp(context.PromptTimeout, TimestampFormat.RelativeTime)}.");
                    return;
                }

                if (!int.TryParse(responses[0], out editRoleMenu))
                {
                    await context.ReplyAsync($"The role menu {Formatter.InlineCode(responses[0])} could not be found. Make sure you're sending a number.");
                    return;
                }
                else if (editRoleMenu < 1 || editRoleMenu > actionRowComponent.Components.Count)
                {
                    await context.ReplyAsync($"The role menu {Formatter.InlineCode(responses[0])} could not be found.");
                    return;
                }

                editRoleMenu--; // Convert to 0-based index
            }

            DiscordComponent component = actionRowComponent.Components.ElementAt(editRoleMenu);
            if (!Guid.TryParse(component.CustomId.Split(':')[0], out Guid roleMenuId))
            {
                await context.ReplyAsync($"Message <{message.JumpLink}> is not a role menu!");
                return;
            }

            RoleMenuModel? roleMenu = _roleMenuService.GetRoleMenu(roleMenuId);
            if (roleMenu is null)
            {
                await context.ReplyAsync($"Message <{message.JumpLink}> is not a role menu!");
                return;
            }

            _roleMenuService.UpdateRoleMenu(roleMenu.Id, roles.Select(role => role.Id));
            await context.ReplyAsync($"Message <{message.JumpLink}> has been updated.");
        }

        [Command("delete")]
        public async Task DeleteAsync(CommandContext context, DiscordMessage message)
        {
            if (message.Author != context.Guild!.CurrentMember)
            {
                await context.ReplyAsync($"I cannot edit message <{message.JumpLink}>.");
                return;
            }
            else if (message.Components.Count == 0)
            {
                await context.ReplyAsync($"Message <{message.JumpLink}> is not a role menu!");
                return;
            }

            DiscordActionRowComponent? actionRowComponent = message.Components.FirstOrDefault();
            if (actionRowComponent is null)
            {
                await context.ReplyAsync($"Message <{message.JumpLink}> is not a role menu!");
                return;
            }

            int deleteRoleMenu = 0;
            if (actionRowComponent.Components.Count > 1)
            {
                IReadOnlyList<string>? responses = await context.PromptAsync(new TextInputComponent("Which role menu would you like to edit?", "1", $"Any number between 1 and {actionRowComponent.Components.Count}", null, true, TextInputStyle.Short, 1, 1));
                if (responses is null)
                {
                    await context.ReplyAsync($"The role menu edit process timed out! Remember that you have to respond within {Formatter.Timestamp(context.PromptTimeout, TimestampFormat.RelativeTime)}.");
                    return;
                }

                if (!int.TryParse(responses[0], out deleteRoleMenu))
                {
                    await context.ReplyAsync($"The role menu {Formatter.InlineCode(responses[0])} could not be found. Make sure you're sending a number.");
                    return;
                }
                else if (deleteRoleMenu < 1 || deleteRoleMenu > actionRowComponent.Components.Count)
                {
                    await context.ReplyAsync($"The role menu {Formatter.InlineCode(responses[0])} could not be found.");
                    return;
                }

                deleteRoleMenu--; // Convert to 0-based index
            }

            DiscordComponent component = actionRowComponent.Components.ElementAt(deleteRoleMenu);
            if (!Guid.TryParse(component.CustomId.Split(':')[0], out Guid roleMenuId))
            {
                await context.ReplyAsync($"Message <{message.JumpLink}> is not a role menu!");
                return;
            }

            RoleMenuModel? roleMenu = _roleMenuService.GetRoleMenu(roleMenuId);
            if (roleMenu is null)
            {
                await context.ReplyAsync($"Message <{message.JumpLink}> is not a role menu!");
                return;
            }
            _roleMenuService.TryRemoveRoleMenu(roleMenu.Id, out _);

            if (actionRowComponent.Components.Count > 1)
            {
                await message.ModifyAsync(new DiscordMessageBuilder().WithContent(message.Content).AddComponents(actionRowComponent.Components.Except(new[] { component })));
            }
            else
            {
                await message.DeleteAsync("Role menu deleted.");
            }
            await context.ReplyAsync($"Message <{message.JumpLink}> has been deleted.");
        }

        [DiscordEvent]
        public Task OnRoleRemoved(DiscordClient client, GuildRoleDeleteEventArgs eventArgs)
        {
            DatabaseContext database = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();
            foreach (RoleMenuModel roleMenu in database.RoleMenus.Where(roleMenu => roleMenu.RoleIds.Contains(eventArgs.Role.Id)))
            {
                roleMenu.RoleIds.Remove(eventArgs.Role.Id);
            };

            return database.SaveChangesAsync();
        }

        [DiscordEvent]
        public async Task OnButtonClickAsync(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
        {
            string[] args = eventArgs.Id.Split(':');
            if (!Guid.TryParse(args[0], out Guid roleMenuId))
            {
                return;
            }

            RoleMenuService roleMenuService = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<RoleMenuService>();
            RoleMenuModel? roleMenu = roleMenuService.GetRoleMenu(roleMenuId);
            if (roleMenu is null)
            {
                return;
            }

            switch (args[1])
            {
                case "list":
                    List<ulong> updatedRoles = new();
                    List<DiscordSelectComponentOption> options = new()
                    {
                        new DiscordSelectComponentOption("None", "0", "Remove all roles from this menu.", false, new("❌"))
                    };

                    foreach (ulong roleId in roleMenu.RoleIds)
                    {
                        DiscordRole? role = eventArgs.Guild.GetRole(roleId);
                        if (role is null)
                        {
                            continue;
                        }

                        updatedRoles.Add(roleId);
                        options.Add(role.Position >= eventArgs.Guild.CurrentMember.Hierarchy
                            ? new(role.Name, roleId.ToString(), "This role is above my highest role. Please contact an admin.", false, new(":x:"))
                            : new(role.Name, role.Id.ToString(), null, ((DiscordMember)eventArgs.User).Roles.Contains(role)));
                    }

                    if (updatedRoles.Count != roleMenu.RoleIds.Count)
                    {
                        roleMenuService.UpdateRoleMenu(roleMenuId, updatedRoles);
                    }

                    await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddComponents(new DiscordSelectComponent($"{roleMenuId}:update", "Select your roles!", options.ToArray(), false, 0, options.Count)).AsEphemeral());
                    return;
                case "update":
                    DiscordMember member = (DiscordMember)eventArgs.User;
                    IEnumerable<ulong> roles = eventArgs.Values.Select(ulong.Parse);

                    if (roles.Any(role => role == 0))
                    {
                        await member.ReplaceRolesAsync(member.Roles.Where(role => !roleMenu.RoleIds.Contains(role.Id)));
                        await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                        {
                            Content = "Since you selected `None` as an option, you no longer have any menu role from this message.",
                            IsEphemeral = true
                        });

                        return;
                    }

                    List<ulong> memberRoles = new(member.Roles.Select(role => role.Id));
                    memberRoles.RemoveAll(role => roleMenu.RoleIds.Contains(role) && !roles.Contains(role));
                    await member.ReplaceRolesAsync(memberRoles.Union(roles).Select(role => eventArgs.Guild.GetRole(role)));

                    await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = "Assigned roles!",
                        IsEphemeral = true
                    });
                    return;
            }
        }
    }
}
