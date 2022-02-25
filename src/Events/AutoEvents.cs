using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tomoe.Attributes;
using Tomoe.Enums;
using Tomoe.Models;

namespace Tomoe.Events
{
    public class AutoEvents
    {
        // Do NOT look at this code unless you're willing to clean it up.
        [SubscribeToEvent(nameof(DiscordShardedClient.MessageCreated))]
        public static async Task MessageCreated(DiscordClient client, MessageCreateEventArgs messageCreateEventArgs)
        {
            if (messageCreateEventArgs.Guild == null || messageCreateEventArgs.Author.IsCurrent)
            {
                return;
            }

            using IServiceScope scope = Program.ServiceProvider.CreateScope() ?? throw new InvalidOperationException("Could not create a ServiceScope.");
            using DatabaseContext database = scope.ServiceProvider.GetRequiredService<DatabaseContext>() ?? throw new InvalidOperationException("DatabaseContext is null.");
            ILogger<AutoEvents> logger = scope.ServiceProvider.GetRequiredService<ILogger<AutoEvents>>() ?? throw new InvalidOperationException("Logger is null.");

            List<IAutoModel> toBeExecuted = new();

            // Filter out auto-events that don't need to be executed
            foreach (IAutoModel autoModel in database.GetAutoModels(messageCreateEventArgs.Guild.Id))
            {
                // We put this if statement here to exit early and prevent our precious CPU cycles from being wasted on Regex.
                if (autoModel.GuildId != messageCreateEventArgs.Guild.Id || autoModel.ChannelId != messageCreateEventArgs.Channel.Id)
                {
                    continue;
                }

                bool executable = false;
                switch (autoModel.FilterType)
                {
                    case FilterType.Attachment:
                        executable = messageCreateEventArgs.Message.Attachments.Count != 0 || messageCreateEventArgs.Message.Embeds.Count != 0;
                        break;
                    case FilterType.Command: // FIXME: Possible lib bug, application command messages aren't sent in the event.
                        executable = messageCreateEventArgs.Message.MessageType is MessageType.ApplicationCommand;
                        break;
                    case FilterType.Embed:
                        executable = messageCreateEventArgs.Message.Embeds.Count != 0;
                        break;
                    case FilterType.File:
                        executable = messageCreateEventArgs.Message.Attachments.Count != 0;
                        break;
                    case FilterType.Phrase:
                        if (autoModel.Filter != null) // It shouldn't be, but it's a safe guard against an NRE
                        {
                            executable = messageCreateEventArgs.Message.Content.Contains(autoModel.Filter);
                        }
                        else
                        {
                            logger.LogWarning("AutoModel {AutoModelId} has a null Filter.", autoModel.Id);
                        }
                        break;
                    case FilterType.Ping:
                        executable = messageCreateEventArgs.Message.MentionedUsers.Count != 0 || messageCreateEventArgs.Message.MentionedRoles.Count != 0 || messageCreateEventArgs.Message.MentionEveryone;
                        break;
                    case FilterType.RolePing:
                        executable = messageCreateEventArgs.Message.MentionedRoles.Count != 0 || messageCreateEventArgs.Message.MentionEveryone;
                        break;
                    case FilterType.UserPing:
                        executable = messageCreateEventArgs.Message.MentionedUsers.Count != 0;
                        break;
                    case FilterType.Regex:
                        if (autoModel.Filter != null) // It shouldn't be, but it's a safe guard against an NRE
                        {
                            executable = Regex.IsMatch(messageCreateEventArgs.Message.Content, autoModel.Filter);
                        }
                        else
                        {
                            logger.LogWarning("AutoModel {AutoModelId} has a null Filter.", autoModel.Id);
                        }
                        break;
                    case FilterType.AllMessages:
                        executable = true;
                        break;
                    default:
                        logger.LogWarning("AutoModel {AutoModelId} has an unknown FilterType.", autoModel.Id);
                        break;
                }

                if (executable)
                {
                    toBeExecuted.Add(autoModel);
                }
            }

            Dictionary<Type, bool> executedTypes = new();
            foreach (IAutoModel autoModel in toBeExecuted)
            {
                Type autoModelType = autoModel.GetType().GenericTypeArguments[0];
                if (!executedTypes.ContainsKey(autoModelType))
                {
                    switch (autoModelType)
                    {
                        case Type when autoModelType.FullName == typeof(IMention).FullName:
                            executedTypes.Add(typeof(IMention), true);
                            List<IMention> mentions = new(toBeExecuted.OfType<AutoModel<IMention>>().SelectMany(x => x.Values).Distinct());

                            StringBuilder mentionString = new(2000);
                            while (mentions.Count > 0)
                            {
                                IMention mention = mentions[0];
                                string stringMention = mentions[0] switch
                                {
                                    RoleMention roleMention => $"<@&{roleMention.Id}>",
                                    UserMention userMention => $"<@{userMention.Id}>",
                                    EveryoneMention => "@everyone",
                                    _ => throw new InvalidOperationException("Invalid IMention type")
                                };

                                if ((mentionString.Length + stringMention.Length) >= 2000)
                                {
                                    // Remove "<mention>, " and replace it with "<mention>."
                                    mentionString.Remove(mentionString.Length - 2, 2);
                                    mentionString.Append('.');
                                    await messageCreateEventArgs.Message.RespondAsync(mentionString.ToString());
                                    mentionString.Clear();
                                }

                                // This makes sure all mentions are sent, not just the ones exceeding the 2000 character limit
                                mentionString.Append(stringMention);
                                if (mentions.Count > 1)
                                {
                                    mentionString.Append(", ");
                                }
                                else
                                {
                                    mentionString.Append('.');
                                    await messageCreateEventArgs.Message.RespondAsync(mentionString.ToString());
                                }

                                mentions.RemoveAt(0);
                            }
                            break;
                        default:
                            logger.LogWarning("AutoModel {AutoModelId} has an unknown Type: {Type}", autoModel.Id, autoModelType.FullName);
                            break;
                    }
                }
            }
        }
    }
}