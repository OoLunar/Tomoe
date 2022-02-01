namespace Tomoe.Commands
{
    using DSharpPlus.SlashCommands;
    using DSharpPlus.SlashCommands.EventArgs;
    using Microsoft.Extensions.DependencyInjection;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Listeners
    {
        public static async Task CommandExecuted(SlashCommandsExtension slashCommandExtension, SlashCommandExecutedEventArgs slashCommandExecutedEventArgs)
        {
            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();
            if (slashCommandExecutedEventArgs.Context.Guild == null)
            {
                return;
            }

            GuildMember guildMember = database.GuildMembers.FirstOrDefault(user => user.UserId == slashCommandExecutedEventArgs.Context.Member.Id && user.GuildId == slashCommandExecutedEventArgs.Context.Guild.Id);
            if (guildMember == null)
            {
                database.AddGuildMember(slashCommandExecutedEventArgs.Context.Member);
                await database.SaveChangesAsync();
            }
            slashCommandExecutedEventArgs.Handled = true;
        }
    }
}