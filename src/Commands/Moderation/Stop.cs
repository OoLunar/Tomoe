using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation
{
    public class OwnerTools : BaseCommandModule
    {
        [Command("stop"), RequireOwner, Description("Shuts down the bot.")]
        public Task Overload(CommandContext context)
        {
            Quit.ConsoleShutdown(null, null);
            return Task.CompletedTask;
        }

        [Command("reboot"), RequireOwner, Description("Restarts the bot.")]

        public Task Overload2(CommandContext context)
        {
#if DEBUG
            Process.Start("dotnet", Environment.GetCommandLineArgs().Prepend("run").Take(2));
#else
            Process.Start(Environment.CommandLine);
#endif
            Quit.ConsoleShutdown(null, null);
            return Task.CompletedTask;
        }
    }
}