using System;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    [Command("base_64")]
    public sealed class Base64Command
    {
        [Command("encode")]
        public static async Task EncodeAsync(CommandContext context, [RemainingText] string text) => await context.RespondAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text)));

        [Command("decode")]
        public static async Task DecodeAsync(CommandContext context, [RemainingText] string text) => await context.RespondAsync(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(text)));
    }
}
