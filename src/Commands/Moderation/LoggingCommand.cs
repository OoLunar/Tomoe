using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Moderation
{
    /// <summary>
    /// Manages logging settings for the guild.
    /// </summary>
    [Command("logging")]
    [RequirePermissions(DiscordPermissions.SendMessages, DiscordPermissions.ManageGuild)]
    public sealed class LoggingCommand
    {
        /// <summary>
        /// Enables logging for the specified event type.
        /// </summary>
        /// <param name="type">The type of event to enable logging for.</param>
        /// <param name="channel">The channel to log the event in.</param>
        /// <param name="format">The format string to use for the event.</param>
        [Command("enable")]
        public static async ValueTask EnableAsync(CommandContext context, GuildLoggingType type, DiscordChannel channel, [RemainingText] string? format = null)
        {
            if (await GuildLoggingModel.GetLoggingAsync(context.Guild!.Id, type) is not GuildLoggingModel logging)
            {
                if (string.IsNullOrWhiteSpace(format))
                {
                    await context.RespondAsync($"This is the first time you are enabling logging for the `{type}` event. Please provide a format string.");
                    return;
                }

                logging = new()
                {
                    GuildId = context.Guild.Id,
                    Enabled = true,
                    Type = type,
                    ChannelId = channel.Id,
                    Format = format
                };
            }
            else
            {
                logging = logging with
                {
                    // Preserve the previous formatting if it wasn't explicitly updated.
                    Format = string.IsNullOrWhiteSpace(format) ? logging.Format : format,
                    Enabled = true
                };
            }

            await GuildLoggingModel.UpsertLoggingAsync(logging);
            await context.RespondAsync($"Logging for the `{type}` event has been enabled in {channel.Mention}.");
        }

        /// <summary>
        /// Disables logging for the specified event type, preserving the configured format.
        /// </summary>
        /// <param name="type">The type of event to disable logging for.</param>
        [Command("disable")]
        public static async ValueTask DisableAsync(CommandContext context, GuildLoggingType type)
        {
            if (await GuildLoggingModel.GetLoggingAsync(context.Guild!.Id, type) is not GuildLoggingModel logging)
            {
                await context.RespondAsync($"Logging for the `{type}` event has not been setup.");
                return;
            }

            await GuildLoggingModel.UpsertLoggingAsync(logging with
            {
                Enabled = false
            });

            await context.RespondAsync($"Logging for the `{type}` event has been disabled.");
        }

        /// <summary>
        /// Changes the format string for the specified event type.
        /// </summary>
        /// <remarks>
        /// If the format string is not provided, the current format string will be displayed.
        /// </remarks>
        /// <param name="type">The type of event to change the format for.</param>
        /// <param name="format">The new format string to use for the event. Leave empty to view the current format.</param>
        [Command("format")]
        public static async ValueTask FormatAsync(CommandContext context, GuildLoggingType type, [RemainingText] string? format = null)
        {
            if (await GuildLoggingModel.GetLoggingAsync(context.Guild!.Id, type) is not GuildLoggingModel logging)
            {
                await context.RespondAsync($"Logging for the `{type}` event has not been setup.");
                return;
            }
            else if (string.IsNullOrWhiteSpace(format))
            {
                await context.RespondAsync($"The current format for the `{type}` event is:\n\n{logging.Format}");
                return;
            }

            await GuildLoggingModel.UpsertLoggingAsync(logging with
            {
                Format = format
            });

            await context.RespondAsync($"The format for the `{type}` event has been updated.");
        }

        /// <summary>
        /// Lists all logging settings for the guild. If a channel is provided, only the logging settings for that channel will be displayed.
        /// </summary>
        /// <param name="channel"></param>
        [Command("list")]
        public static async ValueTask ListAsync(CommandContext context, DiscordChannel? channel = null)
        {
            Dictionary<GuildLoggingType, GuildLoggingModel?> loggings = [];
            foreach (GuildLoggingType type in Enum.GetValues<GuildLoggingType>())
            {
                GuildLoggingModel? logging = await GuildLoggingModel.GetLoggingAsync(context.Guild!.Id, type);
                if (logging is not null && channel is not null && logging.ChannelId != channel.Id)
                {
                    continue;
                }

                loggings[type] = await GuildLoggingModel.GetLoggingAsync(context.Guild!.Id, type);
            }

            if (loggings.Count == 0)
            {
                if (channel is null)
                {
                    await context.RespondAsync("Logging has not been setup yet.");
                }
                else
                {
                    await context.RespondAsync($"No logging settings have been configured for {channel.Mention}.");
                }

                return;
            }

            StringBuilder builder = new();
            foreach ((GuildLoggingType type, GuildLoggingModel? logging) in loggings)
            {
                builder.AppendLine(logging is null
                    ? $"`{type}` is not configured."
                    : $"`{type}` is {(logging.Enabled ? "enabled" : "disabled")} in <#{logging.ChannelId}>.");
            }

            await context.RespondAsync(builder.ToString());
        }
    }
}
