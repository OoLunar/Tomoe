namespace Tomoe.Utilities.Types
{
    using DSharpPlus.Entities;
    using System.Threading.Tasks;

    public class QueueButton
    {
        public string Id { get; private set; }
        public ulong UserId { get; private set; }
        public DiscordComponent[] Components { get; private set; }
        public DiscordComponent SelectedButton { get; set; }

        public QueueButton(string id, ulong userId, params DiscordComponent[] components)
        {
            Id = id;
            UserId = userId;
            Components = components;
            Commands.Listeners.QueueButtons.Add(id, this);
        }

        public async Task<bool> WaitAsync()
        {
            while (SelectedButton == null)
            {
                await Task.Delay(200);
            }

            return SelectedButton.CustomId == (Id + "-confirm");
        }
    }
}