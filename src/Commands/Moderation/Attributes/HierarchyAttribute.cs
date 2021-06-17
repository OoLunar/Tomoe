namespace Tomoe.Commands.Attributes
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class HierarchyAttribute : SlashCheckBaseAttribute
    {
        private readonly bool CanSelfPunish;

        public HierarchyAttribute(bool canSelfPunish = false) => CanSelfPunish = canSelfPunish;

        public override async Task<bool> ExecuteChecksAsync(InteractionContext context)
        {
            if (context.Guild == null)
            {
                return true;
            }

            foreach (DiscordInteractionDataOption val in context.Interaction.Data.Options.Where(option => option.Type.GetType() == typeof(DiscordUser) || option.Type.GetType() == typeof(ulong)))
            {
                DiscordMember discordMember = null;
                if (val.Value is DiscordMember member)
                {
                    discordMember = await member.Id.GetMember(context.Guild);
                }
                else if (val.Value is ulong id)
                {
                    discordMember = await id.GetMember(context.Guild);
                }

                if (discordMember == null)
                {
                    continue;
                }
                else if (discordMember == context.Member && CanSelfPunish)
                {
                    // TODO: Prompt for "self punishment" #How2Masochist
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Error: {discordMember.Mention}'s highest role is greater than or equal to your highest role. You do not have enough power over them!",
                        IsEphemeral = true
                    });
                    return false;
                }
                else if (discordMember.Hierarchy >= context.Member.Hierarchy)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Error: {discordMember.Mention}'s highest role is greater than or equal to your highest role. You do not have enough power over them!",
                        IsEphemeral = true
                    });
                    return false;
                }
                else if (discordMember.Hierarchy >= context.Guild.CurrentMember.Hierarchy)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Error: {discordMember.Mention}'s highest role is greater than or equal to my highest role. I do not have enough power over them!",
                        IsEphemeral = true
                    });
                    return false;
                }
                else if (discordMember.IsOwner)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Error: {discordMember.Mention} is the owner!",
                        IsEphemeral = true
                    });
                    return false;
                }
            }
            return true;
        }
    }
}