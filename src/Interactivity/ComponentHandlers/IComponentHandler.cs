using System.Threading.Tasks;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Interactivity.Data;

namespace OoLunar.Tomoe.Interactivity.ComponentHandlers
{
    public interface IComponentHandler
    {
        public Task HandlePromptAsync(Procrastinator procrastinator, DiscordInteraction interaction, PromptData data);

        public Task HandleConfirmAsync(Procrastinator procrastinator, DiscordInteraction interaction, ConfirmData data);

        public Task HandleChooseAsync(Procrastinator procrastinator, DiscordInteraction interaction, ChooseData data);

        public Task HandleChooseMultipleAsync(Procrastinator procrastinator, DiscordInteraction interaction, ChooseMultipleData data);

        public Task HandleUnknownAsync(Procrastinator procrastinator, DiscordInteraction interaction, IdleData data);

        public Task HandleTimedOutAsync(IdleData data);
    }
}
