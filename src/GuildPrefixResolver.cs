using System;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using Microsoft.Extensions.Caching.Memory;
using OoLunar.Tomoe.Configuration;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe
{
    public sealed class GuildPrefixResolver : IPrefixResolver
    {
        private readonly TomoeConfiguration _configuration;
        private readonly MemoryCache _prefixGuildCache;

        public GuildPrefixResolver(TomoeConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.Discord.Prefix))
            {
                throw new ArgumentException("The global prefix must be set in the configuration.");
            }

            _configuration = configuration;
            _prefixGuildCache = new(new MemoryCacheOptions());
        }

        public async ValueTask<int> ResolvePrefixAsync(CommandsExtension extension, DiscordMessage message)
        {
            // Check for mention prefix
            if (message.Content.StartsWith(extension.Client.CurrentUser.Mention, StringComparison.Ordinal))
            {
                return extension.Client.CurrentUser.Mention.Length;
            }

            // Check for the guild specific prefix, falling back to the global prefix if not set.
            ulong guildId = message.Channel?.Guild?.Id ?? 0;
            string prefix = await _prefixGuildCache.GetOrCreateAsync(guildId, async (entry) =>
            {
                string? prefix = await GuildSettingsModel.GetTextPrefixAsync(guildId);
                entry.SetValue(prefix);
                entry.SetSlidingExpiration(_configuration.Discord.CachePrefixSlidingExpiration);
                return prefix;
            }) ?? _configuration.Discord.Prefix;

            // Check to see if the message starts with the prefix
            return message.Content.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? prefix.Length : -1;
        }

        public void ChangePrefix(ulong guildId, string? prefix) => _prefixGuildCache.Set(guildId, prefix, new MemoryCacheEntryOptions()
        {
            SlidingExpiration = _configuration.Discord.CachePrefixSlidingExpiration
        });
    }
}
