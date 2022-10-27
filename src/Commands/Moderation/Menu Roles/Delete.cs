using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Tomoe.Commands.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands
{
    public partial class Moderation : ApplicationCommandModule
    {
        public partial class MenuRoles : ApplicationCommandModule
        {
            [SlashCommand("delete", "Deletes a menu role."), Hierarchy(Permissions.ManageChannels | Permissions.ManageMessages)]
            public async Task Delete(InteractionContext context, [Option("message_link", "The message to the menu role.")] string messageString)
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new());
                DiscordMessage message;
                DiscordChannel channel = null;
                ulong messageId = 0;
                if (Uri.TryCreate(messageString, UriKind.Absolute, out Uri messageLink))
                {
                    if (messageLink.Host is not "discord.com" and not "discordapp.com")
                    {
                        await context.EditResponseAsync(new()
                        {
                            Content = "The message link must be from Discord."
                        });
                    }
                    else if (messageLink.Segments.Length != 5 || messageLink.Segments[1] != "channels/" || (ulong.TryParse(messageLink.Segments[2].Remove(messageLink.Segments[2].Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out ulong guildId) && guildId != context.Guild.Id))
                    {
                        await context.EditResponseAsync(new()
                        {
                            Content = context.Guild == null ? "Error: Message link isn't from a guild!" : "Error: Message link isn't from this guild!"
                        });
                        return;
                    }
                    else if (ulong.TryParse(messageLink.Segments[3].Remove(messageLink.Segments[3].Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out ulong channelId))
                    {
                        channel = context.Guild.GetChannel(channelId);
                        if (channel == null)
                        {
                            await context.EditResponseAsync(new()
                            {
                                Content = $"Error: Unknown channel <#{channelId}> ({channelId})"
                            });
                            return;
                        }
                        else if (ulong.TryParse(messageLink.Segments[4], NumberStyles.Number, CultureInfo.InvariantCulture, out ulong messageLinkId))
                        {
                            messageId = messageLinkId;
                        }
                    }
                }
                else if (ulong.TryParse(messageString, NumberStyles.Number, CultureInfo.InvariantCulture, out ulong messageIdArgs))
                {
                    channel = context.Channel;
                    messageId = messageIdArgs;
                }
                else
                {
                    await context.EditResponseAsync(new()
                    {
                        Content = $"Error: {messageString} is not a message id or link!"
                    });
                    return;
                }

                message = await channel.GetMessageAsync(messageId);
                if (message == null)
                {
                    await context.EditResponseAsync(new()
                    {
                        Content = $"Error: Unknown message {messageId} ({messageId})"
                    });
                }

                string id = message.Components.First().CustomId.Split('-')[0];
                PermanentButton button = Database.PermanentButtons.FirstOrDefault(permanentButton => permanentButton.ButtonId == id);
                if (button == null)
                {
                    await context.EditResponseAsync(new()
                    {
                        Content = $"Error: Menu role not found!"
                    });
                }

                IEnumerable<MenuRole> menuRoles = Database.MenuRoles.Where(menuRole => menuRole.ButtonId == id);
                if (!menuRoles.Any())
                {
                    await context.EditResponseAsync(new()
                    {
                        Content = $"Error: No menu roles found!"
                    });
                }

                Database.MenuRoles.RemoveRange(menuRoles);
                Database.PermanentButtons.Remove(button);
                await Database.SaveChangesAsync();

                await message.DeleteAsync("Removal of menu role.");
                await context.EditResponseAsync(new()
                {
                    Content = $"Menu roles successfully removed!"
                });
            }
        }
    }
}
