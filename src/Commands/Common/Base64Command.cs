using System;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// A tool for encoding and decoding base64 text.
    /// </summary>
    [Command("base64")]
    public sealed class Base64Command
    {
        /// <summary>
        /// Encodes text into it's base64 form.
        /// </summary>
        /// <param name="text">The text to encode.</param>
        [Command("encode"), DefaultGroupCommand]
        public static ValueTask EncodeAsync(CommandContext context, [RemainingText] string text) => context.RespondAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text)));

        /// <summary>
        /// Decodes base64 text into it's original form.
        /// </summary>
        /// <param name="text">The text to decode.</param>
        [Command("decode")]
        public static ValueTask DecodeAsync(CommandContext context, [RemainingText] string text) => context.RespondAsync(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(text)));
    }
}
