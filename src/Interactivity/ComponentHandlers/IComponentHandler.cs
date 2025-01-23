using System.Threading.Tasks;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Interactivity.Data;

namespace OoLunar.Tomoe.Interactivity.ComponentHandlers
{
    public interface IComponentHandler
    {
        public Task<bool> HandleAnyAsync(Procrastinator procrastinator, DiscordInteraction interaction, IdleData data) => Task.FromResult(true);

        public Task HandlePromptAsync(Procrastinator procrastinator, DiscordInteraction interaction, PromptData data);

        public Task HandleConfirmAsync(Procrastinator procrastinator, DiscordInteraction interaction, ConfirmData data);

        public Task HandlePickAsync(Procrastinator procrastinator, DiscordInteraction interaction, PickData data);

        public Task HandleChooseAsync(Procrastinator procrastinator, DiscordInteraction interaction, ChooseData data);

        public Task HandleUnknownAsync(Procrastinator procrastinator, DiscordInteraction interaction, IdleData data);

        public Task HandleTimedOutAsync(IdleData data);
    }
}
