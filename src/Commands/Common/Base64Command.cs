using System;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;

namespace OoLunar.Tomoe.Commands.Common
{
    [Command("base64")]
    public sealed class Base64Command
    {
        [Command("encode"), DefaultGroupCommand]
        public static ValueTask EncodeAsync(CommandContext context, [RemainingText] string text) => context.RespondAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text)));

        [Command("decode")]
        public static ValueTask DecodeAsync(CommandContext context, [RemainingText] string text) => context.RespondAsync(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(text)));
    }
}
