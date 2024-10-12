using System;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees.Metadata;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// A utility command to interact with Discord snowflakes.
    /// </summary>
    [Command("snowflake")]
    public static class SnowflakeCommand
    {
        /// <summary>
        /// Gets the properties of a snowflake.
        /// </summary>
        /// <param name="snowflake">A message link or literal snowflake.</param>
        [Command("get"), DefaultGroupCommand]
        public static async ValueTask GetAsync(CommandContext context, DiscordSnowflake snowflake)
        {
            StringBuilder builder = new();
            builder.AppendLine($"Value: `{snowflake.Value}`");
            builder.AppendLine($"Timestamp: `{snowflake.Timestamp}`, {Formatter.Timestamp(snowflake.Timestamp)}");
            builder.AppendLine($"Internal Worker ID: `{snowflake.InternalWorkerId}`");
            builder.AppendLine($"Internal Process ID: `{snowflake.InternalProcessId}`");
            builder.AppendLine($"Internal Increment: `{snowflake.InternalIncrement}`");

            await context.RespondAsync(builder.ToString());
        }

        /// <summary>
        /// Creates a snowflake from the data provided and returns its properties through `/snowflake get`.
        /// </summary>
        /// <param name="timestamp">When the snowflake was created at.</param>
        /// <param name="workerId">The worker ID that was used to generate the snowflake.</param>
        /// <param name="processId">The process ID that was used to generate the snowflake.</param>
        /// <param name="increment">How many times the snowflake was generated.</param>
        [Command("create")]
        public static async ValueTask CreateAsync(CommandContext context, DateTimeOffset? timestamp = null, byte? workerId = null, byte? processId = null, ushort? increment = null)
        {
            DiscordSnowflake snowflake = new(timestamp, workerId, processId, increment);
            await GetAsync(context, snowflake);
        }
    }
}
