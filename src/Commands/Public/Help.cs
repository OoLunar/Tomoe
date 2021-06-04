namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public class Help : SlashCommandModule
    {
        [SlashCommand("help", "Sends the help menu for the bot.")]
        public static async Task Command(InteractionContext context, [ChoiceProvider(typeof(TriggerHelpChoiceProvider)), Option("command", "The name of the command to get help on.")] string commandName)
        {
            if (!Api.Public.Commands.TryGetValue(commandName, out MethodInfo command))
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"Error: Command {Formatter.InlineCode(Formatter.Sanitize(commandName))} not found!",
                    IsEphemeral = true
                });
                return;
            }

            SlashCommandAttribute slashCommandAttribute = command.GetCustomAttribute<SlashCommandAttribute>();
            DiscordEmbedBuilder discordEmbedBuilder = new()
            {
                Title = '/' + commandName,
                Description = slashCommandAttribute.Description,
                Color = new DiscordColor("#7b84d1")
            };

            if (context.Guild != null && context.Guild.IconUrl != null)
            {
                discordEmbedBuilder.WithThumbnail(context.Guild.IconUrl);
            }

            foreach (ParameterInfo parameter in command.GetParameters())
            {
                OptionAttribute parameterChoice = parameter.GetCustomAttribute<OptionAttribute>(false);
                if (parameterChoice == null)
                {
                    continue;
                }

                discordEmbedBuilder.AddField((parameter.IsOptional ? "(Optional) " : "(Required) ") + parameterChoice.Name, $"**Type:** {parameter.ParameterType.Name}\n**Description:** {parameterChoice.Description}");
            }

            DiscordInteractionResponseBuilder discordInteractionResponseBuilder = new();
            discordInteractionResponseBuilder.AddEmbed(discordEmbedBuilder);
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, discordInteractionResponseBuilder);
        }
    }

    public class TriggerHelpChoiceProvider : IChoiceProvider
    {
        public Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
        {
            List<DiscordApplicationCommandOptionChoice> discordApplicationCommandOptionChoices = new();
            IEnumerable<Type> commandClasses = Assembly.GetEntryAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(SlashCommandModule)) && !type.IsNested);
            foreach (Type command in commandClasses)
            {
                Api.Public.SearchCommands(command);
            }
            foreach (string commandName in Api.Public.Commands.Keys)
            {
                DiscordApplicationCommandOptionChoice discordApplicationCommandOptionChoice = new(commandName, commandName);
                discordApplicationCommandOptionChoices.Add(discordApplicationCommandOptionChoice);
            }

            discordApplicationCommandOptionChoices.Sort((DiscordApplicationCommandOptionChoice x, DiscordApplicationCommandOptionChoice y) => string.CompareOrdinal(x.Name, y.Name));
            return Task.FromResult(discordApplicationCommandOptionChoices.AsEnumerable());
        }
    }
}