using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace Tomoe.Commands.Public {
    public class ProfilePicture : InteractiveBase {

        /// <summary>
        /// Sends the profile picture of the specified user found by ID.
        /// <code>
        /// >>pfp 336733686529654798
        /// </code>
        /// </summary>
        [Command("pfp", RunMode = RunMode.Async)]
        [Alias("profile_picture")]
        [Remarks("Public")]
        [Summary("[Gets the profile picture of a user or userid.](https://github.com/OoLunar/Tomoe/blob/master/docs/public/pfp.md)")]
        public async Task ByID(ulong user) => await ReplyAsync((await Context.Client.Rest.GetUserAsync(user)).GetAvatarUrl(ImageFormat.Png, 512));

        /// <summary>
        /// Sends the profile picture of the specified user found by mention.
        /// <code>
        /// >>pfp &lt;@336733686529654798&gt;
        /// </code>
        /// </summary>
        [Command("pfp")]
        [Alias("profile_picture")]
        public async Task ByMention(IUser user) => await ReplyAsync((await Context.Client.Rest.GetUserAsync(user.Id)).GetAvatarUrl(ImageFormat.Png, 512));
    }
}