using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class RoleInfoCommand : BaseCommand
    {
        [Command("role_info", "ri")]
        public static Task ExecuteAsync(CommandContext context, DiscordRole role)
        {
            if (context.Guild is null)
            {
                return context.ReplyAsync($"Command `/{context.CurrentCommand.FullName}` can only be used in a guild.");
            }

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = $"Role Info for {role.Name}",
                Author = new()
                {
                    Name = context.Member!.DisplayName,
                    IconUrl = context.User.AvatarUrl,
                    Url = context.User.AvatarUrl
                },
                Color = role.Color.Value == 0x000000 ? Optional.FromNoValue<DiscordColor>() : role.Color
            };
            embedBuilder.AddField("Color", role.Color.ToString(), true);
            embedBuilder.AddField("Created At", Formatter.Timestamp(role.CreationTimestamp.UtcDateTime, TimestampFormat.LongDateTime), true);
            embedBuilder.AddField("Hoisted", role.IsHoisted.ToString(), true);
            embedBuilder.AddField("Is Managed", role.IsManaged.ToString(), true);
            embedBuilder.AddField("Is Mentionable", role.IsMentionable.ToString(), true);
            embedBuilder.AddField("Role Id", role.Id.ToString(CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Role Name", role.Name, true);
            embedBuilder.AddField("Role Position", role.Position.ToMetric(), true);
            embedBuilder.AddField("Permissions", role.Permissions == Permissions.None ? "No permissions." : role.Permissions.ToPermissionString() + ".", false);

            return context.ReplyAsync(new DiscordMessageBuilder().AddEmbed(embedBuilder));
        }
    }
}
