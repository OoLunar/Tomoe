using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Models;

namespace Tomoe.Commands
{
    public sealed class CommandExecutedListener
    {
        public static async Task CommandExecutedAsync(SlashCommandsExtension slashCommandExtension, SlashCommandExecutedEventArgs slashCommandExecutedEventArgs)
        {
            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetRequiredService<Database>();
            if (slashCommandExecutedEventArgs.Context.Guild is null)
            {
                return;
            }

            GuildMember? guildMember = database.GuildMembers.FirstOrDefault(user => user.UserId == slashCommandExecutedEventArgs.Context.Member.Id && user.GuildId == slashCommandExecutedEventArgs.Context.Guild.Id);
            if (guildMember is null)
            {
                database.AddGuildMember(slashCommandExecutedEventArgs.Context.Member);
                await database.SaveChangesAsync();
            }
            slashCommandExecutedEventArgs.Handled = true;
        }
    }
}
