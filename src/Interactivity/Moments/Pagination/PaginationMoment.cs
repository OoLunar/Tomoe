using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Interactivity.Moments.Idle;

namespace OoLunar.Tomoe.Interactivity.Moments.Pagination
{
    public sealed record PaginationMoment : IdleMoment<IPaginationComponentCreator>
    {
        public required IReadOnlyList<Page> Pages { get; init; }
        public int CurrentPageIndex { get; set; }

        public override async ValueTask HandleAsync(Procrastinator procrastinator, DiscordInteraction interaction)
        {
            if (interaction.Message?.Components is null || interaction.Message.Components.Count == 0)
            {
                return;
            }

            string buttonType = interaction.Data.CustomId.Split('_')[1];
            switch (buttonType)
            {
                case "first":
                    CurrentPageIndex = 0;
                    break;
                case "previous":
                    CurrentPageIndex = Math.Max(0, CurrentPageIndex - 1);
                    break;
                case "stop":
                    Page page = Pages[CurrentPageIndex];
                    CurrentPageIndex = -1;
                    await interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, new(page.CreateMessage(this)));
                    return;
                case "next":
                    CurrentPageIndex = Math.Min(CurrentPageIndex + 1, Pages.Count - 1);
                    break;
                case "last":
                    CurrentPageIndex = Pages.Count - 1;
                    break;
                case "dropdown":
                    if (interaction.Data.Values.Length == 1 && int.TryParse(interaction.Data.Values[0], out int pageIndex))
                    {
                        CurrentPageIndex = pageIndex;
                        break;
                    }

                    goto default;
                default:
                    return;
            }

            // Copy the base message, but replace the components with the pagination ones
            DiscordInteractionResponseBuilder responseBuilder = new(Pages[CurrentPageIndex].CreateMessage(this));

            // Readd it to the data
            if (!procrastinator.TryAddData(Id, this))
            {
                // This shouldn't ever happen, but just in case
                throw new InvalidOperationException("The data could not be added to the dictionary.");
            }

            // Update the message
            await interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, responseBuilder);
            Message = interaction.Message ?? await interaction.GetOriginalResponseAsync();

            // Update the timeout
            procrastinator.UpdateTimeout(Id, procrastinator.Configuration.DefaultTimeout);
        }

        public override async ValueTask TimedOutAsync(Procrastinator procrastinator)
        {
            if (Message is null)
            {
                return;
            }

            Page page = Pages[CurrentPageIndex];
            CurrentPageIndex = -1;
            await Message.ModifyAsync(page.CreateMessage(this));
        }
    }
}
