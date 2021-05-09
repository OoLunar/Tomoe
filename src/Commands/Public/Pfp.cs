namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using System.Threading.Tasks;

    public class ProfilePicture : BaseCommandModule
    {
        [Command("profile_picture"), Description("Gets the profile picture of the requested user. Defaults to the requestor when no user is specified."), Aliases("pfp", "avatar")]
        public async Task Overload(CommandContext context) => await Overload(context, context.User, 1024, ImageFormat.Png);

        [Command("profile_picture")]
        public async Task Overload(CommandContext context, [Description("(Optional) The user's pfp to be shown. Defaults to the requestor.")] DiscordUser user, [Description("(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).")] ImageFormat imageFormat = ImageFormat.Png) => await Overload(context, user, 1024, imageFormat);

        [Command("profile_picture")]
        public async Task Overload(CommandContext context, [Description("(Optional) The user's pfp to be shown. Defaults to the requestor.")] DiscordUser user, [Description("(Optional) What size the image should be. Must be a power of two.")] ushort imageSize, [Description("(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).")] ImageFormat imageFormat = ImageFormat.Png)
        {
            bool userExists = user != null;
            if (userExists)
            {
                await Program.SendMessage(context, user.GetAvatarUrl(imageFormat, imageSize));
            }
            else
            {
                await Program.SendMessage(context, Constants.UserNotFound);
            }
        }
    }
}
