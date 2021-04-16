using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

namespace Tomoe
{
	public static class ExtensionMethods
	{
		public static DiscordEmbedBuilder GenerateDefaultEmbed(this DiscordEmbedBuilder embedBuilder, CommandContext context, string title = null)
		{
			if (!string.IsNullOrEmpty(title)) embedBuilder.Title = title.Titleize();
			embedBuilder.Color = new DiscordColor("#7b84d1");
			if (context.Guild == null)
			{
				embedBuilder.Author = new()
				{
					Name = context.User.Username,
					IconUrl = context.User.AvatarUrl,
					Url = context.User.AvatarUrl
				};
			}
			else
			{
				embedBuilder.Author = new()
				{
					Name = context.Member.GetCommonName(),
					IconUrl = context.Member.AvatarUrl,
					Url = context.Member.AvatarUrl
				};
			}
			return embedBuilder;
		}

		public static string GetCommonName(this DiscordMember guildMember) => guildMember == null ? null : guildMember.Nickname ?? guildMember.Username;
	}
}
