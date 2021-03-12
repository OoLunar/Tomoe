using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using Tomoe.Db;
using Tomoe.Utils.Types;

namespace Tomoe.Commands.Moderation
{
	[Group("config"), RequireGuild, Description("Shows and changes settings for the guild.")]
	public class Config : BaseCommandModule
	{
		public enum RoleAction
		{
			Mute,
			Antimeme,
			Voiceban
		}

		public Database Database { private get; set; }

		[GroupCommand, Description("Shows the guild's current config.")]
		public async Task ShowConfig(CommandContext context)
		{
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}

			StringBuilder stringBuilder = new();
			_ = stringBuilder.Append($"Mute Role: {guild.MuteRole.GetRole(context.Guild)?.Mention ?? "Not set"}\n\n");
			_ = stringBuilder.Append($"Antimeme Role: {guild.AntimemeRole.GetRole(context.Guild)?.Mention ?? "Not set"}\n\n");
			_ = stringBuilder.Append($"Voiceban Role: {guild.VoicebanRole.GetRole(context.Guild)?.Mention ?? "Not set"}\n\n");
			string roleMentions = string.Join(", ", guild.AdminRoles.Select(role => role.GetRole(context.Guild)).OrderBy(role => role.Position).Select(role => role.Mention));
			_ = stringBuilder.Append($"Admin Roles: {(roleMentions == string.Empty ? "None set" : roleMentions)}\n\n");
			string allowedInvites = string.Join(", ", guild.AllowedInvites.OrderBy(invite => invite).Select(invite => $"discord.gg/{invite}"));
			_ = stringBuilder.Append($"Allowed Invites: {(allowedInvites == string.Empty ? "None set" : allowedInvites)}\n\n");
			_ = stringBuilder.Append($"AntiInvite: {(guild.AntiInvite ? "Enabled" : "Disabled")}\n\n");
			_ = stringBuilder.Append($"AutoDehoist: {(guild.AutoDehoist ? "Enabled" : "Disabled")}\n\n");
			string channelMentions = string.Join(", ", guild.IgnoredChannels.Select(channel => channel.GetRole(context.Guild)).OrderBy(channel => channel.Position).Select(channel => channel.Mention));
			_ = stringBuilder.Append($"Ignored Channels: {(channelMentions == string.Empty ? "None set" : roleMentions)}\n\n");
			_ = stringBuilder.Append($"Max Lines: {guild.MaxLines}\n\n");
			_ = stringBuilder.Append($"Max Mentions: {guild.MaxMentions}\n\n");
			_ = stringBuilder.Append($"Progressive Strikes: {(guild.ProgressiveStrikes ? "Enabled" : "Disabled")}\n\n");
			string progressiveStrikesAction = string.Join(", ", guild.Punishments.Select(element => $"Strike #{element.Key} => {element.Value}"));
			_ = stringBuilder.Append($"Progressive Strikes Actions: {(progressiveStrikesAction == string.Empty ? "None set" : progressiveStrikesAction)}\n\n");
			_ = stringBuilder.Append($"Automod Strikes: {guild.StrikeAutomod}\n\n");
			InteractivityExtension interactivity = context.Client.GetInteractivity();
			Page[] pages = interactivity.GeneratePagesInEmbed(stringBuilder.ToString(), SplitType.Line, new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"Config For {context.Guild.Name}")).ToArray();
			if (pages.Length == 1)
			{
				_ = await Program.SendMessage(context, null, pages[0].Embed);
			}
			else
			{
				await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);
			}
		}

		[GroupCommand, RequireUserPermissions(Permissions.ManageRoles), RequireBotPermissions(Permissions.ManageChannels), Description("Designates which role should be used for which action. Will additionally change role/channel permissions for said action.")]
		public async Task Roles(CommandContext context, [Description("Either `Mute`, `Antimeme` or `Voiceban`. Case insensitive.")] RoleAction roleAction, [Description("Which role should be used for the `roleAction`.")] DiscordRole discordRole)
		{
			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}

			// GetRole is used in case the role id is 0 (default value) and will either return the Discord role or null
			DiscordRole databaseRole = roleAction switch
			{
				RoleAction.Mute => guild.MuteRole.GetRole(context.Guild),
				RoleAction.Antimeme => guild.AntimemeRole.GetRole(context.Guild),
				RoleAction.Voiceban => guild.VoicebanRole.GetRole(context.Guild),
				_ => null
			};

			// Check to see if a previous role has been set
			if (databaseRole != null && databaseRole.Id != discordRole.Id)
			{
				DiscordMessage confirmRoleOverride = await Program.SendMessage(context, Formatter.Bold($"[Notice: {roleAction} role has already been set. Override it with {discordRole.Mention}?"));
				await new Queue(confirmRoleOverride, context.User, new(async eventArgs =>
				{
					if (eventArgs.Emoji == Constants.ThumbsUp)
					{
						Checklist checklist = new(confirmRoleOverride, "Saving role id to database...", "Override channel permissions for role...");
						switch (roleAction)
						{
							case RoleAction.Mute:
								guild.MuteRole = discordRole.Id;
								break;
							case RoleAction.Antimeme:
								guild.AntimemeRole = discordRole.Id;
								break;
							case RoleAction.Voiceban:
								guild.VoicebanRole = discordRole.Id;
								break;
							default: break;
						}
						_ = await Database.SaveChangesAsync();
						await checklist.Check();
						FixPermissions(context.Guild, roleAction, discordRole);
						await checklist.Finalize($"Role {discordRole.Mention} is now set as the {roleAction} role!");
						checklist.Dispose();
					}
					else if (eventArgs.Emoji == Constants.ThumbsDown)
					{
						_ = await confirmRoleOverride.ModifyAsync(Formatter.Strike(Formatter.Strip(confirmRoleOverride.Content)) + '\n' + Formatter.Bold("[Notice: Aborting!]"));
					}
				})).WaitForReaction();
				return;
			}
			else
			{
				Checklist checklist = new(context, "Saving role id to database...", "Override channel permissions for role...");

				switch (roleAction)
				{
					case RoleAction.Mute:
						guild.MuteRole = discordRole.Id;
						break;
					case RoleAction.Antimeme:
						guild.AntimemeRole = discordRole.Id;
						break;
					case RoleAction.Voiceban:
						guild.VoicebanRole = discordRole.Id;
						break;
					default: break;
				}
				_ = await Database.SaveChangesAsync();
				await checklist.Check();
				FixPermissions(context.Guild, roleAction, discordRole);
				await checklist.Finalize($"Role {discordRole.Mention} is now set as the {roleAction} role!");
				checklist.Dispose();
			}
		}

		[GroupCommand, RequireUserPermissions(Permissions.ManageRoles), RequireBotPermissions(Permissions.ManageChannels | Permissions.ManageRoles), Description("Creates and assigns the proper permissions for the `roleAction`. Creating the role should be preferred so other previous channel overwrites/role permissions do not interfere with the role's purpose.")]
		public async Task Roles(CommandContext context, [Description("Either `Mute`, `Antimeme` or `Voiceban`. Case insensitive.")] RoleAction roleAction)
		{
			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}

			// GetRole is used in case the role id is 0 (default value) and will either return the Discord role or null
			DiscordRole databaseRole = roleAction switch
			{
				RoleAction.Mute => guild.MuteRole.GetRole(context.Guild),
				RoleAction.Antimeme => guild.AntimemeRole.GetRole(context.Guild),
				RoleAction.Voiceban => guild.VoicebanRole.GetRole(context.Guild),
				_ => null
			};

			// Check to see if a previous role has been set
			if (databaseRole != null)
			{
				_ = await Program.SendMessage(context, Formatter.Bold($"[Notice: {roleAction} role is already set!]"));
				return;
			}
			else
			{
				// Get the correct rolename for those pesky grammar elitests.
				string rolename = roleAction switch
				{
					RoleAction.Mute => "Muted",
					RoleAction.Antimeme => "Antimemed",
					RoleAction.Voiceban => "Voicebanned",
					_ => "Unknown"
				};
				Checklist checklist = new(context, $"Creating role {Formatter.InlineCode(rolename)}...", "Saving role id to database...", "Override channel permissions for role...");
				DiscordRole role = await context.Guild.CreateRoleAsync(rolename, Permissions.None, DiscordColor.Gray, false, false, $"Creating {roleAction} role.");
				await checklist.Check();
				switch (roleAction)
				{
					case RoleAction.Mute:
						guild.MuteRole = role.Id;
						break;
					case RoleAction.Antimeme:
						guild.AntimemeRole = role.Id;
						break;
					case RoleAction.Voiceban:
						guild.VoicebanRole = role.Id;
						break;
					default: break;
				}
				_ = await Database.SaveChangesAsync();
				await checklist.Check();
				FixPermissions(context.Guild, roleAction, role);
				await checklist.Finalize($"Role {role.Mention} is now set as the {roleAction} role!");
				checklist.Dispose();
			}
		}

		[Command("anti_invite"), Aliases("antiinvite", "antinvite", "remove_invites", "removeinvites"), RequireUserPermissions(Permissions.ManageMessages), Description("Determines whether invites should be allowed to be posted or not.")]
		public async Task AntiInvite(CommandContext context)
		{
			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}

			guild.AntiInvite = !guild.AntiInvite;
			_ = await Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, $"Invites will now be {(guild.AntiInvite ? "removed" : "kept")} when posted.");
		}

		[Command("auto_dehoist"), Aliases("autodehoist", "dehoist"), RequireUserPermissions(Permissions.ManageNicknames), Description("Determines whether nicknames should be allowed at the top of the list or not.")]
		public async Task AutoDehoist(CommandContext context)
		{
			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}

			guild.AutoDehoist = !guild.AutoDehoist;
			_ = await Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, $"Hoisted nicknames will now be {(guild.AntiInvite ? $"renamed to {Formatter.InlineCode("dehoisted")}" : "kept")}.");
		}

		[Command("max_lines"), Aliases("maxlines", "max_line", "maxline"), RequireUserPermissions(Permissions.ManageMessages), Description("Sets the limit on the max newlines allowed on messages.")]
		public async Task MaxLines(CommandContext context, [Description("The maximum amount of lines allowed in a message.")] int maxLineCount)
		{
			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}
			guild.MaxLines = maxLineCount;
			_ = await Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, $"The maximum lines allowed in a message is now {maxLineCount}.");
		}

		[Command("max_mentions"), Aliases("maxmentions"), RequireUserPermissions(Permissions.ManageMessages), Description("Sets the maximum mentions allowed in a message. User pings and role pings are added together for the total ping count, which determines if the user gets a strike or not.")]
		public async Task MaxMentions(CommandContext context, [Description("The maximum amount of role and user pings allowed in a message.")] int maxMentionCount)
		{
			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}
			guild.MaxMentions = maxMentionCount;
			_ = await Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, $"The maximum mentions allowed in a message is now {maxMentionCount}.");
		}

		[Command("add_invite"), Aliases("addinvite", "allow_invite", "allowinvite"), RequireUserPermissions(Permissions.ManageMessages), Description("Adds a Discord invite to the whitelist. Only effective if `anti_invite` is enabled.")]
		public async Task AddInvite(CommandContext context, [Description("The Discord invite to whitelist.")] DiscordInvite discordInvite)
		{
			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}

			if (guild.AllowedInvites.Contains(discordInvite.Code))
			{
				_ = await Program.SendMessage(context, $"Invite discord.gg/{discordInvite.Code} was already whitelisted!");
			}
			else
			{
				guild.AllowedInvites.Add(discordInvite.Code);
				_ = await Database.SaveChangesAsync();
				_ = await Program.SendMessage(context, $"Invite discord.gg/{discordInvite.Code} is now whitelisted.");
			}
		}

		[Command("remove_invite"), Aliases("removeinvite", "delete_invite", "deleteinvite"), RequireUserPermissions(Permissions.ManageMessages), Description("Removes an invite from the whitelist. Only effective if `anti_invite` is enabled.")]
		public async Task RemoveInvite(CommandContext context, [Description("The Discord invite to whitelist.")] DiscordInvite discordInvite)
		{
			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}

			if (guild.AllowedInvites.Remove(discordInvite.Code))
			{
				_ = await Database.SaveChangesAsync();
				_ = await Program.SendMessage(context, "Invite has been removed from the whitelist.");
			}
			else
			{
				_ = await Program.SendMessage(context, "Invite was not whitelisted!");
			}
		}

		[Command("ignore_channel"), Aliases("ignorechannel", "hide_channel", "hidechannel"), RequireUserPermissions(Permissions.ManageChannels), Description("Prevents the bot from reading messages and executing commands in the specified channel.")]
		public async Task IgnoreChannel(CommandContext context, [Description("The Discord channel to ignore.")] DiscordChannel discordChannel)
		{
			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}

			if (guild.IgnoredChannels.Contains(discordChannel.Id))
			{
				_ = await Program.SendMessage(context, $"Invite discord.gg/{discordChannel.Mention} was already whitelisted!");
			}
			else
			{
				guild.IgnoredChannels.Add(discordChannel.Id);
				_ = await Database.SaveChangesAsync();
				_ = await Program.SendMessage(context, $"Invite discord.gg/{discordChannel.Mention} is now whitelisted.");
			}
		}

		[Command("unignore_channel"), Aliases("unignorechannel", "show_channel", "showchannel"), RequireUserPermissions(Permissions.ManageChannels), Description("Allows the bot to see messages and execute commands in the specified channel.")]
		public async Task UnignoreChannel(CommandContext context, [Description("The Discord invite to unignore.")] DiscordChannel discordChannel)
		{
			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}

			if (guild.IgnoredChannels.Remove(discordChannel.Id))
			{
				_ = await Database.SaveChangesAsync();
				_ = await Program.SendMessage(context, "The channel is now shown.");
			}
			else
			{
				_ = await Program.SendMessage(context, "The channel wasn't hidden!");
			}
		}

		[Command("add_admin"), Aliases("addadmin", "add_staff", "addstaff", "admin", "staff"), RequireUserPermissions(Permissions.ManageGuild), Description("Adds the specified role to the staff list. Staff roles are exempt from all automoderation.")]
		public async Task AddAdmin(CommandContext context, [Description("The Discord role to set as admin.")] DiscordRole discordRole)
		{
			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}

			if (guild.AdminRoles.Contains(discordRole.Id))
			{
				_ = await Program.SendMessage(context, $"The role {discordRole.Mention} was already on the staff list!");
			}
			else
			{
				guild.AdminRoles.Add(discordRole.Id);
				_ = await Database.SaveChangesAsync();
				_ = await Program.SendMessage(context, $"The role {discordRole.Mention} is now considered staff.");
			}
		}

		[Command("remove_admin"), Aliases("removeadmin", "remove_staff", "removestaff", "unadmin", "unstaff"), RequireUserPermissions(Permissions.ManageGuild), Description("Removes the specified role from the staff list.")]
		public async Task RemoveAdmin(CommandContext context, [Description("The Discord role to remove from admin.")] DiscordRole discordRole)
		{
			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}

			if (guild.AdminRoles.Remove(discordRole.Id))
			{
				_ = await Database.SaveChangesAsync();
				_ = await Program.SendMessage(context, "The role is no longer considered staff.");
			}
			else
			{
				_ = await Program.SendMessage(context, "The role wasn't on the staff list!");
			}
		}

		[Command("strike_automod"), Aliases("strikeautomod", "auto_strike", "autostrike"), RequireUserPermissions(Permissions.ManageMessages), Description("Determines whether automod should add a strike to the victim.")]
		public async Task StrikeAutomod(CommandContext context)
		{
			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}

			guild.StrikeAutomod = !guild.StrikeAutomod;
			_ = await Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, $"Automod will {(guild.AntiInvite ? "now" : "no longer")} issue strikes.");
		}

		[Command("progressive_strikes"), Aliases("progressivestrikes", "auto_punishments", "autopunishments"), RequireUserPermissions(Permissions.ManageMessages), Description("Determines whether punishments should lead into other punishments depending on the total amount of strikes the victim has already accumulated.")]
		public async Task ProgressiveStrikes(CommandContext context)
		{
			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}

			guild.StrikeAutomod = !guild.StrikeAutomod;
			_ = await Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, $"Progressive punishments will {(guild.AntiInvite ? "now" : "no longer")} be in service.");
		}

		/// <summary>
		/// Fixes a <see cref="DiscordRole"/>'s <see cref="Permissions"/> for the entire <paramref name="discordGuild"/> to ensure that the <paramref name="discordRole"/> works as intended.
		/// </summary>
		/// <param name="discordGuild">The <see cref="DiscordGuild"/> in question.</param>
		/// <param name="roleAction">The <see cref="RoleAction"/> determines which set of <see cref="Permissions"/> to use.</param>
		/// <param name="discordRole">The <see cref="DiscordRole"/> whose <see cref="Permissions"/> should be fixed.</param>
		public static void FixPermissions(DiscordGuild discordGuild, RoleAction roleAction, DiscordRole discordRole) => discordGuild.Channels.Values.ToList().ForEach(async channel => await FixPermissions(channel, roleAction, discordRole));

		/// <summary>
		/// Fixes a <see cref="DiscordRole"/>'s <see cref="Permissions"/> for a specific <paramref name="discordChannel"/> to ensure that the <paramref name="discordRole"/> works as intended.
		/// </summary>
		/// <param name="discordChannel">The <see cref="DiscordChannel"/> in question.</param>
		/// <param name="roleAction">The <see cref="RoleAction"/> determines which set of <see cref="Permissions"/> to use.</param>
		/// <param name="discordRole">The <see cref="DiscordRole"/> whose <see cref="Permissions"/> should be fixed.</param>
		public static async Task FixPermissions(DiscordChannel discordChannel, RoleAction roleAction, DiscordRole discordRole)
		{
			Permissions textChannelPerms = roleAction switch
			{
				RoleAction.Mute => Permissions.SendMessages | Permissions.AddReactions,
				RoleAction.Antimeme => Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis,
				RoleAction.Voiceban => Permissions.None,
				_ => Permissions.None
			};
			Permissions voiceChannelPerms = roleAction switch
			{
				RoleAction.Mute => Permissions.Speak | Permissions.Stream,
				RoleAction.Antimeme => Permissions.Stream | Permissions.UseVoiceDetection,
				RoleAction.Voiceban => Permissions.UseVoice,
				_ => Permissions.None
			};
			Permissions categoryPerms = textChannelPerms | voiceChannelPerms;

			string auditLogReason = roleAction switch
			{
				RoleAction.Mute => "Configuring permissions for mute role. Preventing role from sending messages, reacting to messages and speaking in voice channels.",
				RoleAction.Antimeme => "Configuring permissions for antimeme role. Preventing role from reacting to messages, embedding links and uploading files. In voice channels, preventing role from streaming and forcing push-to-talk.",
				RoleAction.Voiceban => "Configuring permissions for voiceban role. Preventing role from connecting to voice channels.",
				_ => "Not configuring unknown role action."
			};

			switch (discordChannel.Type)
			{
				case ChannelType.Voice:
					await discordChannel.AddOverwriteAsync(discordRole, Permissions.None, voiceChannelPerms, auditLogReason);
					break;
				case ChannelType.Category:
					await discordChannel.AddOverwriteAsync(discordRole, Permissions.None, categoryPerms, auditLogReason);
					break;
				default:
					await discordChannel.AddOverwriteAsync(discordRole, Permissions.None, textChannelPerms, auditLogReason);
					break;
			}
		}
	}
}
