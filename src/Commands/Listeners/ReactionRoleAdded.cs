namespace Tomoe.Commands.Listeners
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using Microsoft.Extensions.DependencyInjection;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public class ReactionRoleAdded
    {
        public static async Task Handler(DiscordClient client, MessageReactionAddEventArgs eventArgs)
        {
            if (eventArgs.User.Id == client.CurrentUser.Id)
            {
                return;
            }

            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();

            ReactionRole reactionRole = database.ReactionRoles.FirstOrDefault(databaseReactionRole
                => databaseReactionRole.GuildId == eventArgs.Guild.Id
                && databaseReactionRole.MessageId == eventArgs.Message.Id
                && databaseReactionRole.EmojiName == eventArgs.Emoji.GetDiscordName()
            );
            // Reaction role doesn't exist, meaning it's just a random reaction.
            if (reactionRole == null)
            {
                return;
            }

            DiscordRole discordRole = eventArgs.Guild.GetRole(reactionRole.RoleId);
            // if the discord role has been removed, remove the reaction role from the database.
            if (discordRole == null)
            {
                _ = database.ReactionRoles.Remove(reactionRole);
                _ = await database.SaveChangesAsync();
                return;
            }

            // Get the user and give them their reaction role.
            await (await eventArgs.User.Id.GetMember(eventArgs.Guild)).GrantRoleAsync(discordRole);
        }
    }
}
