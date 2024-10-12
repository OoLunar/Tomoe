using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Converters
{
    public sealed class DiscordSnowflakeConverter : ITextArgumentConverter<DiscordSnowflake>, ISlashArgumentConverter<DiscordSnowflake>
    {
        public string ReadableName => "Discord Snowflake - A very large number or a Discord message link, Discord channel mention, Discord user mention, or Discord role mention.";
        public ConverterInputType RequiresText => ConverterInputType.Always;
        public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.String;

        public Task<Optional<DiscordSnowflake>> ConvertAsync(ConverterContext context)
        {
            if (context.Argument is ulong snowflake)
            {
                return Task.FromResult(Optional.FromValue(new DiscordSnowflake(snowflake)));
            }

            string? value = context.Argument?.ToString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return Task.FromResult(Optional.FromNoValue<DiscordSnowflake>());
            }
            else if (ulong.TryParse(value, out ulong snowflakeValue))
            {
                return Task.FromResult(Optional.FromValue(new DiscordSnowflake(snowflakeValue)));
            }

            // Try to see if it's a message link.
            Match match = DiscordMessageConverter.GetMessageRegex().Match(value);
            if (match.Success && ulong.TryParse(match.Groups["message"].ValueSpan, out ulong messageId))
            {
                return Task.FromResult(Optional.FromValue(new DiscordSnowflake(messageId)));
            }

            // Try to see if it's a channel mention.
            match = DiscordChannelConverter.GetChannelMatchingRegex().Match(value);
            if (match.Success && ulong.TryParse(match.Groups[1].ValueSpan, out ulong channelId))
            {
                return Task.FromResult(Optional.FromValue(new DiscordSnowflake(channelId)));
            }

            // Try to see if it's a user mention.
            match = DiscordUserConverter.GetMemberRegex().Match(value);
            if (match.Success && ulong.TryParse(match.Groups[1].ValueSpan, out ulong userId))
            {
                return Task.FromResult(Optional.FromValue(new DiscordSnowflake(userId)));
            }

            // Try to see if it's a role mention.
            match = DiscordRoleConverter.GetRoleRegex().Match(value);
            return match.Success && ulong.TryParse(match.Groups[1].ValueSpan, out ulong roleId)
                ? Task.FromResult(Optional.FromValue(new DiscordSnowflake(roleId)))
                : Task.FromResult(Optional.FromNoValue<DiscordSnowflake>());
        }
    }
}
