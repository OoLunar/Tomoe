using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Interactivity.Moments.Idle;

namespace OoLunar.Tomoe.Interactivity.Moments.Choose
{
    public record ChooseMoment : IdleMoment<IChooseComponentCreator>
    {
        public required string Question { get; init; }
        public required IReadOnlyList<string> Options { get; init; }
        public TaskCompletionSource<IReadOnlyList<string>> TaskCompletionSource { get; init; } = new();

        public override async ValueTask HandleAsync(Procrastinator procrastinator, DiscordInteraction interaction)
        {
            if (interaction.Type != DiscordInteractionType.Component || interaction.Message?.Components is null || interaction.Message.Components.Count == 0)
            {
                return;
            }

            TaskCompletionSource.SetResult(interaction.Data.Values);

            DiscordInteractionResponseBuilder responseBuilder = new(new DiscordMessageBuilder(interaction.Message));
            responseBuilder.ClearComponents();
            foreach (DiscordActionRowComponent row in interaction.Message.Components.Mutate<DiscordSelectComponent>(
                select => select.CustomId.StartsWith(Id.ToString(), StringComparison.Ordinal),
                select =>
                {
                    List<DiscordSelectComponentOption> options = [];
                    foreach (DiscordSelectComponentOption option in select.Options)
                    {
                        options.Add(new DiscordSelectComponentOption(
                            option.Label,
                            option.Value,
                            option.Description,
                            option.Value == interaction.Data.Values[0],
                            option.Emoji
                        ));
                    }

                    return new DiscordSelectComponent(select.CustomId, select.Placeholder, options, true, select.MinimumSelectedValues ?? 1, select.MaximumSelectedValues ?? 1);
                }
            ).Cast<DiscordActionRowComponent>())
            {
                responseBuilder.AddActionRowComponent(row);
            }

            await interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, responseBuilder);
        }

        public override ValueTask TimedOutAsync(Procrastinator procrastinator)
        {
            TaskCompletionSource.SetResult([]);
            return base.TimedOutAsync(procrastinator);
        }
    }
}
