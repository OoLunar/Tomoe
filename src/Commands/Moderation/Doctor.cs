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

namespace Tomoe.Commands.Moderation
{
    public class Doctor : BaseCommandModule
    {
        [Command("doctor"), Description("Shows which commands are missing their required permissions."), RequireGuild]
        public Task DoctorAsync(CommandContext context)
        {
            List<DiscordEmbedBuilder> embeds = new();
            DiscordEmbedBuilder builder = new();

            if (context.Guild.CurrentMember.Permissions.HasFlag(Permissions.Administrator))
            {
                builder.Description = "You have the Administrator permission, so you can use all commands. It is advised you re-invite the bot with the proper permissions for a security boost. The `invite` command will give you the link with the correct permissions.";
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

                    builder.AddField(commandName, Formatter.BlockCode(string.Join('\n', Enum.GetValues<Permissions>().Where(x => commandPerms.HasFlag(x)).Select(x => $"+ {x.Humanize()}")), "diff"), true);
                }
            }
            else
            {
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

                    builder.AddField(commandName, Formatter.BlockCode(string.Join('\n', Enum.GetValues<Permissions>().Where(x => x != Permissions.None && commandPerms.HasFlag(x)).Select(x => (context.Guild.CurrentMember.Permissions.HasFlag(x) ? "+ " : "- ") + x.Humanize())), "diff"), true);
                }
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
