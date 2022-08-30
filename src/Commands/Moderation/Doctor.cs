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
    public sealed class Doctor : BaseCommandModule
    {
        [Command("doctor")]
        [Description("Checks if the bot has the required permissions it's commands.")]
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

            foreach ((string commandName, Command command) in context.CommandsNext.RegisteredCommands)
            {
                Permissions commandPerms = (Permissions)command.ExecutionChecks.OfType<RequirePermissionsAttribute>().Select(x => (long)x.Permissions).Sum();
                commandPerms |= (Permissions)command.ExecutionChecks.OfType<RequireBotPermissionsAttribute>().Select(x => (long)x.Permissions).Sum();

                if (commandPerms == 0)
                {
                    continue;
                }
                else if (builder.Fields.Count == 25)
                {
                    embeds.Add(builder);
                    builder = new();
                }

                builder.AddField(commandName, Formatter.BlockCode(string.Join('\n', Enum.GetValues<Permissions>().Where(x => x != Permissions.None && commandPerms.HasPermission(x)).Select(x => (context.Guild.CurrentMember.Permissions.HasPermission(x) ? "+ " : "- ") + x.Humanize())), "diff"), true);
            }

            if (builder.Fields.Count != 0)
            {
                embeds.Add(builder);
            }

            return embeds.Count == 1
                ? context.RespondAsync(embeds[0])
                : context.Client.GetInteractivity().SendPaginatedMessageAsync(context.Channel, context.User, embeds.Select(x => new Page(null, x)));
        }
    }
}
