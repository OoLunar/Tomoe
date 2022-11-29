using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;

namespace OoLunar.Tomoe.Commands.Moderation
{
    /// <summary>
    /// Informs the user which permissions the bot is missing in the guild. Additionally includes which commands the bot cannot use.
    /// </summary>
    public sealed class DoctorCommand : BaseCommandModule
    {
        /// <summary>
        /// Informs the user which permissions the bot is missing in the guild. Additionally includes which commands the bot cannot use.
        /// </summary>
        /// <param name="context">The context that the command was used in.</param>
        [Command("doctor")]
        [Description("Informs the user which permissions the bot is missing in the guild. Additionally includes which commands the bot cannot use.")]
        [RequireGuild]
        public Task DoctorAsync(CommandContext context)
        {
            List<DiscordEmbedBuilder> embeds = new();
            DiscordEmbedBuilder builder = new()
            {
                Description = context.Guild.CurrentMember.Permissions.HasFlag(Permissions.Administrator)
                    ? "I have the Administrator permission, I can execute all of my commands without issue. It is advised you re-invite me with the proper permissions for a security boost. The `invite` command will give you the link with the correct permissions."
                    : "The red permissions are the permissions that I do not have. The green permissions are the ones I do have. If a command has a red permission, that means I cannot execute it."
            };

            // Iterate through the registered commands
            foreach ((string commandName, Command command) in context.CommandsNext.RegisteredCommands)
            {
                // Join the permissions from the RequirePermissionsAttribute and RequireBotPermissionsAttribute.
                Permissions commandPerms = (Permissions)command.ExecutionChecks.OfType<RequirePermissionsAttribute>().Select(x => (long)x.Permissions).Sum();
                commandPerms |= (Permissions)command.ExecutionChecks.OfType<RequireBotPermissionsAttribute>().Select(x => (long)x.Permissions).Sum();

                // No permissions required, skip.
                if (commandPerms == 0)
                {
                    continue;
                }
                // New embed.
                else if (builder.Fields.Count == 25)
                {
                    embeds.Add(builder);
                    builder = new();
                }

                builder.AddField(commandName, Formatter.BlockCode(string.Join('\n', Enum.GetValues<Permissions>().Where(x => x != Permissions.None && commandPerms.HasPermission(x)).Select(x => (context.Guild.CurrentMember.Permissions.HasPermission(x) ? "+ " : "- ") + x.Humanize())), "diff"), true);
            }

            // Add the last embed.
            if (builder.Fields.Count != 0)
            {
                embeds.Add(builder);
            }

            // Paginate the embeds for readability.
            return embeds.Count == 1
                ? context.RespondAsync(embeds[0])
                : context.Client.GetInteractivity().SendPaginatedMessageAsync(context.Channel, context.User, embeds.Select(x => new Page(null, x)));
        }
    }
}
