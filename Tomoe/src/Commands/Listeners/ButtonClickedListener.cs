using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Models;
using Tomoe.Utilities.Types;

namespace Tomoe.Commands
{
    public sealed class ButtonClickedListener
    {
        public static Dictionary<string, QueueButton> QueueButtons { get; private set; } = new();

        public static async Task ButtonClickedAsync(DiscordClient discordClient, ComponentInteractionCreateEventArgs componentInteractionCreateEventArgs)
        {
            string id = componentInteractionCreateEventArgs.Id.Split('-')[0];
            if (int.TryParse(componentInteractionCreateEventArgs.Id.Split('-')[1], NumberStyles.Number, CultureInfo.InvariantCulture, out int stage))
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetRequiredService<Database>();
                PermanentButton? button = database.PermanentButtons.FirstOrDefault(button => button.ButtonId == id && button.GuildId == componentInteractionCreateEventArgs.Guild.Id);
                if (button is not null)
                {
                    switch (button.ButtonType)
                    {
                        case ButtonType.MenuRole:
                            componentInteractionCreateEventArgs.Handled = true;
                            if (stage == 1)
                            {
                                await Moderation.MenuRoleCommand.AssignAsync(componentInteractionCreateEventArgs.Interaction, await componentInteractionCreateEventArgs.User.Id.GetMemberAsync(componentInteractionCreateEventArgs.Guild)!, componentInteractionCreateEventArgs.Id, database);
                            }
                            else if (stage == 2)
                            {
                                await Moderation.MenuRoleCommand.AssignAsync(componentInteractionCreateEventArgs, componentInteractionCreateEventArgs.Id, database);
                            }
                            break;
                        default:
                            return;
                    }
                }
            }
            else
            {
                if (QueueButtons.TryGetValue(id, out QueueButton? queueButton) && queueButton.UserId == componentInteractionCreateEventArgs.User.Id)
                {
                    queueButton.SelectedButton = queueButton.Components.First(discordComponent => discordComponent.CustomId == componentInteractionCreateEventArgs.Id);
                    QueueButtons.Remove(id);
                    await componentInteractionCreateEventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate, new());
                    componentInteractionCreateEventArgs.Handled = true;
                }
            }

        }
    }
}