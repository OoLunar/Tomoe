using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class TimestampCommand : BaseCommand
    {
        [Command("timestamp")]
        public static Task ExecuteAsync(CommandContext context, TimestampFormat format = TimestampFormat.LongDateTime, TimeSpan? when = null) => ExecuteAsync(context, format, when.HasValue ? DateTime.UtcNow.Add(when.Value) : DateTime.UtcNow);

        [Command("timestamp")]
        public static Task ExecuteAsync(CommandContext context, TimestampFormat format = TimestampFormat.LongDateTime, DateTime? when = null) => context.ReplyAsync($"Timestamp: {Formatter.Timestamp(when ?? DateTime.UtcNow, format)}");
    }
}
