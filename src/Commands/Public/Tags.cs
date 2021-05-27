namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    [SlashCommandGroup("tag", "Sends a predetermined message.")]
    public class Tags : SlashCommandModule
    {
        public Database Database { private get; set; }

        [SlashCommand("send", "Sends a pretermined message.")]
        public async Task Send(InteractionContext context, [Option("tag_name", "The name of the tag to send")] string tagName)
        {
            Tag tag = await GetTagAsync(tagName);
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
        public async Task Create(InteractionContext context, [Option("tag_name", "What to call the new tag.")] string tagName, [Option("tag_content", "What to fill the new tag with.")] string tagContent)
        {
            if ((await GetTagAsync(tagName)) != null)
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
        public async Task Delete(InteractionContext context, [Option("tag_name", "Which tag to remove permanently.")] string tagName)
        {
            Tag tag = await GetTagAsync(tagName);
            if (tag == null)
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"Tag `{tagName.ToLowerInvariant()}` already exists!",
                    IsEphemeral = true
                });
                return;
            }

            if (tag.OwnerId == context.User.Id)
            {
                Database.Tags.Remove(tag);
                await Database.SaveChangesAsync();
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"Tag `{tag.Name}` successfully deleted!"
                });
            }
            else
            {
                DiscordMember discordMember = await context.User.Id.GetMember(context.Guild);
                if (discordMember.HasPermission(Permissions.ManageMessages))
                {
                    Database.Tags.Remove(tag);
                    await Database.SaveChangesAsync();
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Tag `{tag.Name}` successfully deleted!"
                    });
                    return;
                }

                // See if the user has any admin roles
                GuildConfig guildConfig = Database.GuildConfigs.First(guildConfig => guildConfig.Id == context.Guild.Id);
                if (guildConfig.AdminRoles.Intersect(discordMember.Roles.Select(role => role.Id)).Any())
                {
                    Database.Tags.Remove(tag);
                    await Database.SaveChangesAsync();
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Tag `{tag.Name}` successfully deleted!"
                    });
                    return;
                }
                else
                {
                    Database.Tags.Remove(tag);
                    await Database.SaveChangesAsync();
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"You don't have permission to delete tag `{tag.Name}`!",
                        IsEphemeral = true
                    });
                }
            }
        }

        public async Task<Tag> GetTagAsync(string tagName)
        {
            tagName = tagName.ToLowerInvariant();
            Tag tag = Database.Tags.FirstOrDefault(databaseTag => databaseTag.Name == tagName);

            // Test if the tag is an alias
            if (tag != null && tag.IsAlias)
            {
                // Retrieve the original tag
                tag = Database.Tags.FirstOrDefault(databaseTag => databaseTag.Name == tag.AliasTo);
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
    }
}