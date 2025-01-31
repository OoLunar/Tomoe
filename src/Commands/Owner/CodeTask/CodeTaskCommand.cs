using System;
using System.Net.Http;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;

namespace OoLunar.Tomoe.Commands.Owner
{
    /// <summary>
    /// A plugin system designed specifically for the bot owner.
    /// </summary>
    [Command("codetask"), RequireApplicationOwner, RequireGuild]
    public sealed partial class CodeTaskCommand
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Creates a new <see cref="CodeTaskCommand"/> with the specified <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> to use for downloading files.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use for dependency injection.</param>
        public CodeTaskCommand(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _serviceProvider = serviceProvider;
        }
    }
}
