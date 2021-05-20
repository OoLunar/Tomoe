namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;
    using DSharpPlus.Interactivity.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Tomoe.Db;

    [Group("reaction_roles"), Description("Assigns a role to the user(s) who react to a certain message."), Aliases("rr", "reaction_role"), RequirePermissions(Permissions.ManageRoles | Permissions.AddReactions)]
    public class ReactionRoles : BaseCommandModule
    {
        public Database Database { private get; set; }

        [GroupCommand]
        public async Task ByMessage(CommandContext context, [Description("The message to put a reaction role on.")] DiscordMessage message, [RemainingText, Description("Should be formatted as such (exact emojis or roles not required): :heart: @Role1 :blue_heart: @Role2 :green_heart: @Role1 :orange_heart: @Role3")] string emojiRoleList)
        {
            try
            {
                if (string.IsNullOrEmpty(emojiRoleList))
                {
                    await Program.SendMessage(context, Formatter.Bold($"[Error]: `:emoji: @Role` format is required!"));
                }
                else if (await Api.Moderation.ReactionRoles.Create(context.Client, context.Guild, context.User.Id, message, emojiRoleList))
                {
                    await Program.SendMessage(context, $"Reaction roles created!");
                }
                else
                {
                    await Program.SendMessage(context, Formatter.Bold($"[Error]: Message {message.Id} already has those reaction roles!"));
                }
            }
            catch (FormatException)
            {
                await Program.SendMessage(context, Formatter.Bold($"[Error]: Improperly formatted input. The input should be in the following format: :heart: @Role1 :blue_heart: @Role2 :green_heart: @Role1 :orange_heart: @Role3, etc. One or more emoji-role pair is required."));
            }
        }

        [GroupCommand]
        public async Task ByReply(CommandContext context, [RemainingText, Description("Should be formatted as such (exact emojis or roles not required): :heart: @Role1 :blue_heart: @Role2 :green_heart: @Role1 :orange_heart: @Role3")] string emojiRoleList)
        {
            if (context.Message.ReferencedMessage != null)
            {
                await ByMessage(context, context.Message.ReferencedMessage, emojiRoleList);
            }
            else
            {
                await ByMessage(context, (await context.Channel.GetMessagesAsync(2))[1], emojiRoleList);
            }
        }

        [GroupCommand, Description("Assigns a role to the user(s) who react to the last message in the channel.")]
        public async Task LastMessage(CommandContext context, [Description("Which channel to get the last message from.")] DiscordChannel channel, [RemainingText, Description("Should be formatted as such (exact emojis or roles not required): :heart: @Role1 :blue_heart: @Role2 :green_heart: @Role1 :orange_heart: @Role3")] string emojiRoleList) => await ByMessage(context, (await channel.GetMessagesAsync(1))[0], emojiRoleList);

        [Command("fix"), Description("Adds Tomoe's reactions back onto previous reaction role messages."), Aliases("repair", "rereact")]
        public async Task Fix(CommandContext context, [Description("Which channel needs to be fixed.")] DiscordChannel channel)
        {
            if (await Api.Moderation.ReactionRoles.Fix(context.Client, context.Guild, context.User.Id, channel))
            {
                await Program.SendMessage(context, $"All reaction roles in channel {channel.Mention} have been fixed!");
            }
            else
            {
                await Program.SendMessage(context, "No reaction roles needed to be fixed!");
            }
        }

        [Command("fix")]
        public async Task Fix(CommandContext context, [Description("Which message needs to be fixed.")] DiscordMessage message)
        {
            if (await Api.Moderation.ReactionRoles.Fix(context.Client, context.Guild, context.User.Id, message))
            {
                await Program.SendMessage(context, $"Fixed all reaction roles on message <{message.JumpLink}>");
            }
            else
            {
                await Program.SendMessage(context, "No reaction roles needed to be fixed!");
            }
        }

        [Command("list"), Description("Shows all the current reaction roles on a message"), Aliases("show", "ls")]
        public async Task List(CommandContext context, [Description("Gets all reaction roles on this message.")] DiscordMessage message)
        {
            StringBuilder stringBuilder = new();
            foreach (ReactionRole reactionRole in Api.Moderation.ReactionRoles.Get(context.Guild.Id, context.Channel, message.Id))
            {
                stringBuilder.AppendLine($"{DiscordEmoji.FromName(context.Client, reactionRole.EmojiName, true)} => {context.Guild.GetRole(reactionRole.RoleId).Mention}");
            }

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"Reaction Roles on Message");
            embedBuilder.Title += ' ' + context.Message.JumpLink.ToString();
            if (stringBuilder.Length <= 2000)
            {
                embedBuilder.Description = stringBuilder.ToString();
                await Program.SendMessage(context, null, embedBuilder.Build());
            }
            else
            {
                InteractivityExtension interactivity = context.Client.GetInteractivity();
                IEnumerable<Page> pages = interactivity.GeneratePagesInEmbed(stringBuilder.ToString(), DSharpPlus.Interactivity.Enums.SplitType.Line, embedBuilder);
                await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);
            }
        }

        [Command("remove"), Description("Deletes a reaction role from a message."), Aliases("delete", "rm", "del")]
        public async Task Remove(CommandContext context, [Description("Which message to remove the reaction role from.")] DiscordMessage message, [Description("The emoji that's with the role.")] DiscordEmoji emoji)
        {
            if (await Api.Moderation.ReactionRoles.Delete(context.Client, context.Guild, context.User.Id, message, emoji))
            {
                await Program.SendMessage(context, $"The reaction role has been deleted!");
            }
            else
            {
                await Program.SendMessage(context, Formatter.Bold($"[Error]: No reaction role was found with {emoji} on message <{message.JumpLink}>"));
            }
        }
    }
}
