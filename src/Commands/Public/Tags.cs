namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using Microsoft.Extensions.DependencyInjection;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public class Tags : SlashCommandModule
    {
        public override Task BeforeExecutionAsync(InteractionContext context)
        {
            if (context.Guild == null)
            {
                context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = "Error: This command can only be used in a guild!",
                    IsEphemeral = true
                });
            }

            return Task.CompletedTask;
        }

        [SlashCommandGroup("tag", "Sends a predetermined message.")]
        public class RealTags : SlashCommandModule
        {
            public Database Database { private get; set; }

            [SlashCommand("send", "Sends a pretermined message.")]
            public async Task Send(InteractionContext context, [Option("name", "The name of the tag to send")] string tagName)
            {
                Tag tag = await GetTagAsync(tagName, context.Guild.Id);
                if (tag == null)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Tag `{tagName.ToLowerInvariant()}` was not found!",
                        IsEphemeral = true
                    });
                }
                else
                {
                    tag.Uses++;
                    await Database.SaveChangesAsync();
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    {
                        Content = tag.Content
                    });
                }
            }

            [SlashCommand("create", "Creates a new tag.")]
            public async Task Create(InteractionContext context, [Option("name", "What to call the new tag.")] string tagName, [Option("tag_content", "What to fill the new tag with.")] string tagContent)
            {
                if ((await GetTagAsync(tagName, context.Guild.Id)) != null)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Tag `{tagName.ToLowerInvariant()}` already exists!",
                        IsEphemeral = true
                    });
                    return;
                }

                Tag tag = new();
                tag.Name = tagName.ToLowerInvariant();
                tag.Content = tagContent;
                tag.GuildId = context.Guild.Id;
                tag.OwnerId = context.User.Id;
                tag.TagId = Database.Tags.Count(databaseTag => databaseTag.GuildId == context.Guild.Id) + 1;
                Database.Tags.Add(tag);
                await Database.SaveChangesAsync();
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"Created tag `{tag.Name}` with content:\n{tag.Content}"
                });
            }

            [SlashCommand("delete", "Deletes a tag.")]
            public async Task Delete(InteractionContext context, [Option("name", "Which tag to remove permanently.")] string tagName)
            {
                Tag tag = await GetTagAsync(tagName, context.Guild.Id);
                if (tag == null)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Tag `{tagName.ToLowerInvariant()}` does not exist!",
                        IsEphemeral = true
                    });
                    return;
                }

                if (await CanModifyTag(tag, context.User.Id, context.Guild))
                {
                    IEnumerable<Tag> tags = Database.Tags.Where(databaseTag => databaseTag.AliasTo == tag.Name);
                    Database.Tags.RemoveRange(tags.Append(tag));
                    await Database.SaveChangesAsync();
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Tag `{tag.Name}` successfully deleted!"
                    });
                }
                else
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"You don't have permission to delete tag `{tag.Name}`!",
                        IsEphemeral = true
                    });
                }
            }

            [SlashCommand("edit", "Edits a tag's text.")]
            public async Task Edit(InteractionContext context, [Option("name", "Which tag to edit.")] string tagName, [Option("Tag_Content", "What to fill the tag with.")] string tagContent)
            {
                Tag tag = await GetTagAsync(tagName, context.Guild.Id);
                if (tag == null)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Tag `{tagName.ToLowerInvariant()}` does not exist!",
                        IsEphemeral = true
                    });
                    return;
                }

                if (await CanModifyTag(tag, context.User.Id, context.Guild))
                {
                    tag.Content = tagContent;
                    await Database.SaveChangesAsync();
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Tag `{tag.Name}` successfully edited!\n{tag.Content}"
                    });
                }
                else
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"You don't have permission to edit tag `{tag.Name}`!",
                        IsEphemeral = true
                    });
                }
            }

            [SlashCommand("alias", "Points one tag to another.")]
            public async Task Alias(InteractionContext context, [Option("New_Tag", "What to call the new alias.")] string newTagName, [Option("Old_Tag", "Which tag to point to.")] string oldTagName)
            {
                Tag newTag = await GetTagAsync(newTagName, context.Guild.Id);
                if (newTag != null)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Tag `{newTagName.ToLowerInvariant()}` already exists!",
                        IsEphemeral = true
                    });
                    return;
                }
                Tag oldTag = await GetTagAsync(oldTagName, context.Guild.Id);
                if (oldTag == null)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Tag `{oldTagName.ToLowerInvariant()}` does not exist!",
                        IsEphemeral = true
                    });
                    return;
                }

                Tag alias = new();
                alias.AliasTo = oldTag.Name;
                alias.GuildId = context.Guild.Id;
                alias.IsAlias = true;
                alias.Name = newTagName.ToLowerInvariant();
                alias.OwnerId = context.User.Id;
                alias.TagId = Database.Tags.Count(databaseTag => databaseTag.GuildId == context.Guild.Id) + 1;
                Database.Tags.Add(alias);
                await Database.SaveChangesAsync();
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"Alias `{alias.Name}` now points to `{oldTag.Name}`!",
                });
            }

            [SlashCommand("purge_alias", "Removes all aliases from a tag.")]
            public async Task PurgeAlias(InteractionContext context, [Option("name", "Which tag to remove all aliases from.")] string tagName)
            {
                Tag tag = await GetTagAsync(tagName, context.Guild.Id);
                if (tag == null)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Tag `{tagName.ToLowerInvariant()}` does not exist!",
                        IsEphemeral = true
                    });
                    return;
                }
                else if (tag.IsAlias)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Tag `{tagName.ToLowerInvariant()}` is an alias! Use {Formatter.InlineCode($"/tag info {tag.Name}")} to get the original tag name!",
                        IsEphemeral = true
                    });
                    return;
                }

                if (await CanModifyTag(tag, context.User.Id, context.Guild))
                {
                    IEnumerable<Tag> tags = Database.Tags.Where(databaseTag => databaseTag.AliasTo == tag.Name);
                    Database.Tags.RemoveRange(tags);
                    await Database.SaveChangesAsync();
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Aliases for tag `{tag.Name}` successfully deleted!"
                    });
                }
                else
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"You don't have permission to remove aliases for tag `{tag.Name}`!",
                        IsEphemeral = true
                    });
                }
            }

            [SlashCommand("transfer", "Transfer ownership of a tag to another person.")]
            public async Task Transfer(InteractionContext context, [Option("name", "Which tag to transfer")] string tagName, [Option("New_Tag_Owner", "Who to transfer the tag too.")] DiscordUser newTagOwner = null)
            {
                newTagOwner ??= context.User;
                Tag tag = await GetTagAsync(tagName, context.Guild.Id);
                if (tag == null)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Tag `{tagName.ToLowerInvariant()}` does not exist!",
                        IsEphemeral = true
                    });
                    return;
                }

                if (await CanModifyTag(tag, context.User.Id, context.Guild))
                {
                    tag.OwnerId = newTagOwner.Id;
                    await Database.SaveChangesAsync();
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Tag `{tag.Name}` has been transferred to {newTagOwner.Mention}!"
                    });
                }
                else
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"You don't have permission to transfer tag `{tag.Name}`!",
                        IsEphemeral = true
                    });
                }
            }

            [SlashCommand("info", "Sends general information on the requested tag.")]
            public async Task Info(InteractionContext context, [Option("name", "Which tag to gather information on.")] string tagName)
            {
                Tag tag = await GetTagAsync(tagName, context.Guild.Id);
                if (tag == null)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Tag `{tagName.ToLowerInvariant()}` does not exist!",
                        IsEphemeral = true
                    });
                    return;
                }

                DiscordEmbedBuilder embedBuilder = new();
                embedBuilder.Color = new DiscordColor("#7b84d1");
                embedBuilder.Title = "Information on tag " + tag.Name;
                embedBuilder.AddField("Is An Alias", tag.IsAlias.ToString());
                if (tag.IsAlias)
                {
                    embedBuilder.AddField("Alias To", tag.AliasTo);
                }
                else
                {
                    embedBuilder.AddField("Aliases", string.Join(", ", Database.Tags.Where(databaseTag => databaseTag.AliasTo == tag.Name).Select(databaseTag => databaseTag.Name)));
                    embedBuilder.Description = tag.Content;
                }
                embedBuilder.AddField("Created At", tag.CreatedAt.ToOrdinalWords());
                embedBuilder.AddField("Global Id", $"`{tag.Id}`");
                embedBuilder.AddField("Tag Name", tag.Name);
                embedBuilder.AddField("Owner", $"<@{tag.OwnerId}>");
                embedBuilder.AddField("Local Id", '#' + tag.TagId.ToString(CultureInfo.InvariantCulture));
                embedBuilder.AddField("Total Uses", tag.Uses.ToMetric());

                DiscordInteractionResponseBuilder messageBuilder = new();
                messageBuilder.AddEmbed(embedBuilder);
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, messageBuilder);
            }

            [SlashCommand("author", "Gets the author of a tag.")]
            public async Task Author(InteractionContext context, [Option("name", "Which tag to gather information on.")] string tagName)
            {
                Tag tag = await GetTagAsync(tagName, context.Guild.Id);
                if (tag == null)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Tag `{tagName.ToLowerInvariant()}` does not exist!",
                        IsEphemeral = true
                    });
                }
                else
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"<@{tag.OwnerId}>"
                    });
                }
            }

            public async Task<Tag> GetTagAsync(string tagName, ulong guildId)
            {
                if (Database == null)
                {
                    IServiceScope scope = Program.ServiceProvider.CreateScope();
                    Database = scope.ServiceProvider.GetService<Database>();
                }

                tagName = tagName.ToLowerInvariant();
                Tag tag = Database.Tags.FirstOrDefault(databaseTag => databaseTag.Name == tagName && databaseTag.GuildId == guildId);

                // Test if the tag is an alias
                if (tag != null && tag.IsAlias)
                {
                    // Retrieve the original tag
                    tag = Database.Tags.FirstOrDefault(databaseTag => databaseTag.Name == tag.AliasTo && databaseTag.GuildId == guildId);
                    // This shouldn't happen since Tags.Delete should also delete aliases, but it's here for safety.
                    if (tag == null)
                    {
                        Database.Tags.Remove(tag);
                        await Database.SaveChangesAsync();
                        return null;
                    }
                }
                return tag;
            }

            public async Task<bool> CanModifyTag(Tag tag, ulong memberId, DiscordGuild guild)
            {
                if (tag.OwnerId == memberId)
                {
                    return true;
                }

                if (guild.OwnerId == memberId)
                {
                    return true;
                }

                // Tag creators should have permission over their tag's aliases.
                if (tag.IsAlias)
                {
                    Tag originalTag = Database.Tags.FirstOrDefault(databaseTag => databaseTag.AliasTo == tag.Name && databaseTag.GuildId == guild.Id);
                    if (originalTag == null)
                    {
                        return true;
                    }
                    else if (originalTag.OwnerId == memberId)
                    {
                        return true;
                    }
                }

                DiscordMember discordMember = await memberId.GetMember(guild);
                if (discordMember == null)
                {
                    return true;
                }
                else if (discordMember.HasPermission(Permissions.ManageMessages))
                {
                    return true;
                }

                GuildConfig guildConfig = Database.GuildConfigs.First(guildConfig => guildConfig.Id == guild.Id);
                return guildConfig.AdminRoles.Intersect(discordMember.Roles.Select(role => role.Id)).Any();
            }
        }
    }
}