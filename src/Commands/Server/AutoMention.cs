using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.Extensions.Logging;
using Tomoe.Enums;

namespace Tomoe.Commands.Server
{
    [Group("auto_mention"), Description("Automatically mentions a user or role upon a match to a message's content."), RequireGuild, RequireBotPermissions(Permissions.SendMessages | Permissions.ReadMessageHistory | Permissions.SendMessagesInThreads | Permissions.AccessChannels)]
    public class AutoMention : AutoCommand<IMention>
    {
        [Command("add")]
        public override Task AddAsync(CommandContext context, DiscordChannel channel, params IMention[] value) => base.AddAsync(context, channel, value);

        [Command("add")]
        public override Task AddAsync(CommandContext context, DiscordChannel channel, FilterType filterType, string? filter, params IMention[] value) => base.AddAsync(context, channel, filterType, filter, value);

        [Command("remove"), RequireUserPermissions(Permissions.ManageMessages)]
        public override Task RemoveAsync(CommandContext context, params Guid[] ids) => base.RemoveAsync(context, ids);

        [Command("remove"), RequireUserPermissions(Permissions.ManageMessages)]
        public override Task RemoveAsync(CommandContext context, params DiscordChannel[] channels) => base.RemoveAsync(context, channels);

        [Command("remove"), RequireUserPermissions(Permissions.ManageMessages)]
        public override Task RemoveAsync(CommandContext context, [RemainingText] params KeyValuePair<FilterType, string?>[] filterTypes) => base.RemoveAsync(context, filterTypes);

        [Command("remove"), RequireUserPermissions(Permissions.ManageMessages)]
        public override Task RemoveAsync(CommandContext context, params IMention[] values) => base.RemoveAsync(context, values);

        [Command("list"), RequireUserPermissions(Permissions.ManageMessages)]
        public override Task ListAsync(CommandContext context) => base.ListAsync(context);

        [Command("list"), RequireUserPermissions(Permissions.ManageMessages)]
        public override Task ListAsync(CommandContext context, [RemainingText] params Guid[] ids) => base.ListAsync(context, ids);

        [Command("list"), RequireUserPermissions(Permissions.ManageMessages)]
        public override Task ListAsync(CommandContext context, [RemainingText] params DiscordChannel[] channels) => base.ListAsync(context, channels);

        [Command("list"), RequireUserPermissions(Permissions.ManageMessages)]
        public override Task ListAsync(CommandContext context, [RemainingText] params KeyValuePair<FilterType, string?>[] filterTypes) => base.ListAsync(context, filterTypes);

        [Command("list"), RequireUserPermissions(Permissions.ManageMessages)]
        public override Task ListAsync(CommandContext context, [RemainingText] IMention mention) => base.ListAsync(context, mention);

        public override async Task BeforeExecutionAsync(CommandContext ctx)
        {
            switch (ctx.Command!.Name)
            {
                case "add":
                    DiscordChannel channel = (DiscordChannel)await ctx.CommandsNext.ConvertArgument<DiscordChannel>(ctx.RawArguments[0], ctx);
                    Permissions memberChannelPermissions = ctx.Member!.PermissionsIn(channel);
                    Permissions botChannelPermissions = ctx.Guild.CurrentMember.PermissionsIn(channel);
                    string channelOrThread = channel.IsThread ? "thread" : "channel";

                    // User perms
                    if (!memberChannelPermissions.HasPermission(Permissions.SendMessages | Permissions.SendMessagesInThreads))
                    {
                        await ctx.RespondAsync($"You don't have permission to send messages in this {channelOrThread}.");
                        throw new InvalidOperationException("Permissions check failed.");
                    }
                    else if (!memberChannelPermissions.HasPermission(Permissions.ManageMessages))
                    {
                        await ctx.RespondAsync($"You don't have permission to manage messages in this {channelOrThread}.");
                        throw new InvalidOperationException("Permissions check failed.");
                    }
                    else if ((ctx.Message.MentionedRoles.Count != 0 || ctx.Message.MentionEveryone) && !memberChannelPermissions.HasPermission(Permissions.MentionEveryone))
                    {
                        await ctx.RespondAsync($"You don't have permission to mention roles in this {channelOrThread}.");
                        throw new InvalidOperationException("Permissions check failed.");
                    }

                    // Bot perms
                    else if (!botChannelPermissions.HasPermission(Permissions.SendMessages | Permissions.SendMessagesInThreads))
                    {
                        await ctx.RespondAsync($"I don't have permission to send messages in this {channelOrThread}.");
                        throw new InvalidOperationException("Permissions check failed.");
                    }
                    else if ((ctx.Message.MentionedRoles.Count != 0 || ctx.Message.MentionEveryone) && !botChannelPermissions.HasPermission(Permissions.MentionEveryone))
                    {
                        await ctx.RespondAsync($"I don't have permission to mention roles in this {channelOrThread}.");
                        throw new InvalidOperationException("Permissions check failed.");
                    }

                    // Make sure we don't break our rate limits.
                    else if (Database.AutoMentions.Count(x => x.GuildId == ctx.Guild.Id) >= 10)
                    {
                        await ctx.RespondAsync("You can only have 10 auto mentions per guild.");
                        throw new InvalidOperationException("Maximum auto mentions reached.");
                    }
                    break;
                case "remove":
                    if (ctx.Overload.Arguments[0].Type == typeof(Guid))
                    {
                        Guid[] ids = await Task.WhenAll(ctx.RawArguments[0].Split(' ').Select(async x => (Guid)await ctx.CommandsNext.ConvertArgument<Guid>(x, ctx)));
                        if (ids.Length == 0)
                        {
                            await ctx.RespondAsync("You must specify at least one ID.");
                            throw new InvalidOperationException("No IDs specified.");
                        }

                        Guid[] databaseIds = Database.AutoMentions.Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToArray();
                        IEnumerable<Guid> unknownIds = ids.Except(databaseIds);
                        if (unknownIds.Any())
                        {
                            await ctx.RespondAsync($"Unknown IDs: {string.Join(", ", unknownIds)}");
                            throw new InvalidOperationException("Unknown IDs.");
                        }
                    }
                    else if (ctx.Overload.Arguments[0].Type == typeof(DiscordChannel))
                    {
                        DiscordChannel[] channels = await Task.WhenAll(ctx.RawArguments[0].Split(' ').Select(async x => (DiscordChannel)await ctx.CommandsNext.ConvertArgument<DiscordChannel>(x, ctx)));
                        if (channels.Length == 0)
                        {
                            await ctx.RespondAsync("You must specify at least one channel.");
                            throw new InvalidOperationException("No channels specified.");
                        }
                        ulong[] channelIds = channels.Select(x => x.Id).ToArray();

                        int foundAutoMentions = Database.AutoMentions.Count(x => channelIds.Contains(x.ChannelId));
                        if (foundAutoMentions == 0)
                        {
                            await ctx.RespondAsync($"No auto mentions found in {channels.Humanize()}.");
                            throw new InvalidOperationException("Unknown channels.");
                        }
                    }
                    else if (ctx.Overload.Arguments[0].Type == typeof(KeyValuePair<FilterType, string?>[]))
                    {
                        KeyValuePair<FilterType, string?>[] filterTypes = await Task.WhenAll(ctx.RawArguments[0].Split(' ').Select(async x => (KeyValuePair<FilterType, string?>)await ctx.CommandsNext.ConvertArgument<KeyValuePair<FilterType, string?>>(x, ctx)));
                        if (filterTypes.Length == 0)
                        {
                            await ctx.RespondAsync("You must specify at least one filter type.");
                            throw new InvalidOperationException("No filter types specified.");
                        }

                        foreach (KeyValuePair<FilterType, string?> filterType in filterTypes)
                        {
                            if (filterType.Key is FilterType.Regex or FilterType.Phrase && filterType.Value is null)
                            {
                                await ctx.RespondAsync($"Filter type {filterType.Key.Humanize()} must have a value.");
                                throw new InvalidOperationException("No filter type value specified.");
                            }
                        }

                        int foundAutoMentions = Database.AutoMentions.Count(x => filterTypes.Any(y => y.Key == x.FilterType && y.Value == x.Filter));
                        if (foundAutoMentions == 0)
                        {
                            await ctx.RespondAsync($"No auto mentions found with the specified filters.");
                            throw new InvalidOperationException("Unknown filters.");
                        }
                    }
                    else if (ctx.Overload.Arguments[0].Type == typeof(IMention))
                    {
                        IMention[] values = await Task.WhenAll(ctx.RawArguments[0].Split(' ').Select(async x => (IMention)await ctx.CommandsNext.ConvertArgument<IMention>(x, ctx)));
                        if (values.Length == 0)
                        {
                            await ctx.RespondAsync("You must specify at least one value.");
                            throw new InvalidOperationException("No values specified.");
                        }

                        int foundAutoMentions = Database.AutoMentions.Count(x => x.Values.SequenceEqual(values));
                        if (foundAutoMentions == 0)
                        {
                            await ctx.RespondAsync($"No auto mentions found with the specified values.");
                            throw new InvalidOperationException("Unknown values.");
                        }
                    }
                    else // This happens when there's a new Remove Overload in the AutoCommand class that we haven't checked.
                    {
                        await ctx.RespondAsync("Invalid argument type.");
                        throw new InvalidOperationException("Unknown remove overload.");
                    }
                    break;
                default:
                    Logger.LogWarning("{AutoMention} received an unknown command: {CommandName}", nameof(AutoMention), ctx.Command.Name);
                    break;
            }
        }

        public override async Task AfterExecutionAsync(CommandContext ctx)
        {
            switch (ctx.Command!.Name)
            {
                case "add":
                    int mentionCount = ctx.Message.MentionedUsers.Count + ctx.Message.MentionedRoles.Count + (ctx.Message.MentionEveryone ? 1 : 0);
                    await ctx.RespondAsync($"Added {mentionCount.ToMetric()} auto mention{(mentionCount != 1 ? 's' : null)}.");
                    break;
                case "remove":
                    if (ctx.Overload.Arguments[0].Type == typeof(Guid))
                    {
                        mentionCount = ctx.RawArguments[0].Split(' ').Length;
                        await ctx.RespondAsync($"Removed {mentionCount.ToMetric()} auto mention{(mentionCount != 1 ? 's' : null)}.");
                    }
                    else if (ctx.Overload.Arguments[0].Type == typeof(DiscordChannel))
                    {
                        await ctx.RespondAsync($"Cleared all auto mentions in the following channels: {ctx.RawArgumentString}.");
                    }
                    else if (ctx.Overload.Arguments[0].Type == typeof(KeyValuePair<FilterType, string?>[]))
                    {
                        await ctx.RespondAsync($"Cleared all auto mentions with the following filters: {ctx.RawArgumentString}.");
                    }
                    else if (ctx.Overload.Arguments[0].Type == typeof(IMention))
                    {
                        await ctx.RespondAsync($"Cleared all auto mentions with the following values: {ctx.RawArgumentString}.");
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
