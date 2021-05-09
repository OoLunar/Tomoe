namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using LibGit2Sharp;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Utils;
    using Tomoe.Utils.Types;

    public class Update : BaseCommandModule
    {
        [Command("update"), RequireOwner, Description("Updates the bot to the latest version. Branch is set in the config.")]
        public async Task ByUser(CommandContext context)
        {
            await Program.SendMessage(context, "Broken. See https://github.com/libgit2/libgit2sharp/issues/1883");
            string latestVersion = GetLatestVersion().FriendlyName.ToLowerInvariant();
            if (latestVersion == Constants.Version.ToLowerInvariant())
            {
                await Program.SendMessage(context, "Currently on the latest version!");
            }
            else
            {
                if (Program.Config.Update.AutoUpdate)
                {
                    Checklist checklist = new(context, "Downloading latest version...", "Rebooting...");
                    Download();
                    await checklist.Finalize("Rebooting", false);
                    System.Diagnostics.Process.Start(Environment.GetCommandLineArgs()[0], string.Join(' ', Environment.GetCommandLineArgs().Skip(1)));
                    Quit.ConsoleShutdown(null, null);
                }
                else
                {
                    DiscordMessage message = await Program.SendMessage(context, $"Latest version: {latestVersion}. Current version: {Constants.Version}. Would you like to update? Be aware that the files will be downloaded to the current directory and not the project directory!");
                    Queue queue = new(message, context.User, new(async eventArgs =>
                    {
                        if (eventArgs.TimedOut || eventArgs.MessageReactionAddEventArgs.Emoji == Constants.ThumbsDown)
                        {
                            await message.ModifyAsync(Formatter.Strike(Formatter.Strip(message.Content)) + "\nNot updating!");
                        }
                        else
                        {
                            Checklist checklist = new(context, "Downloading latest version...", "Rebooting");
                            Download();
                            await checklist.Finalize("Rebooting", false);
                            System.Diagnostics.Process.Start(Environment.GetCommandLineArgs()[0], string.Join(' ', Environment.GetCommandLineArgs().Skip(1)));
                            Quit.ConsoleShutdown(null, null);
                        }
                    }));
                }
            }
        }

        public static Tag GetLatestVersion()
        {
            using Repository repo = new("./");
            return repo.Tags
                .OrderByDescending(tag => tag.FriendlyName) // Order by version: 2.0.0, 1.2.0, 1.0.0, etc
                .First(tag => Program.Config.Update.Branch != "public" || !tag.FriendlyName.Contains("beta")); // If the branch is not public, then return the latest tag. Else, give me a tag that doesn't include "beta" in the name
        }

        public static void Download()
        {
            using Repository repo = new("./");
            Tag latestTag = GetLatestVersion();
            CherryPickOptions cherryPickOptions = new();
            cherryPickOptions.FailOnConflict = false;
            cherryPickOptions.FileConflictStrategy = CheckoutFileConflictStrategy.Theirs;
            cherryPickOptions.FindRenames = true;
            cherryPickOptions.IgnoreWhitespaceChange = false;
            repo.CherryPick((Commit)latestTag.Target, new Signature(Program.Config.Update.GitName, Program.Config.Update.GitEmail, DateTimeOffset.Now), cherryPickOptions);
        }
    }
}
