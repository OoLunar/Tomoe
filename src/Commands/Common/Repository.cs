using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Configuration;

namespace Tomoe.Commands.Common
{
	public class Repository : BaseCommandModule
	{
		public IConfiguration Configuration { private get; init; } = null!;

		[Command("repository"), Description("Sends the source code for Tomoe."), Aliases("github", "gh", "gitlab", "repo", "code")]
		public Task RepositoryAsync(CommandContext context) => context.RespondAsync(Formatter.EmbedlessUrl(Configuration.GetValue("repository_link", new Uri("https://github.com/OoLunar/Tomoe"))));
	}
}
