using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace Tomoe.Commands.Public {
    public class Help : InteractiveBase {
        [Command("help", RunMode = RunMode.Async)]
        [Summary("[Sends the help menu, and links to the GitHub documentation.](https://github.com/OoLunar/Tomoe/blob/master/docs/public/help.md)")]
        [Remarks("Public")]
        public async Task help() {
            List<CommandInfo> commands = Program.Commands.Commands.ToList();
            EmbedBuilder embedBuilder = new EmbedBuilder();
            string publicCommands = null;
            string moderatorCommands = "__All commands can be used through mentions or ID's.__\n\n";
            foreach (CommandInfo command in commands) {
                switch (command.Remarks) {
                    case "Public":
                        publicCommands += $"{command.Name}: {command.Summary ?? "No description available."}\n\n";
                        break;
                    case "Moderation":
                        moderatorCommands += $"{command.Name}: {command.Summary ?? "No description available."}\n\n";
                        break;
                    default:
                        continue;
                }
            }

            embedBuilder.AddField("Public Commands", publicCommands ?? "None available.");
            embedBuilder.AddField("Moderator Commands", moderatorCommands);

            await ReplyAsync("Here's a list of commands and their description: ", false, embedBuilder.Build());
        }
    }
}