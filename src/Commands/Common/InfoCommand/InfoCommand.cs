using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Configuration;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Shows general information about whatever the user wants.
    /// </summary>
    [Command("info"), RequirePermissions(DiscordPermissions.EmbedLinks, DiscordPermissions.None)]
    public sealed partial class InfoCommand
    {
        private readonly AllocationRateTracker _allocationRateTracker = new();
        private readonly ImageUtilities _imageUtilitiesService;
        private readonly TomoeConfiguration _configuration;

        /// <summary>
        /// Creates a new instance of <see cref="InfoCommand"/>.
        /// </summary>
        /// <param name="imageUtilitiesService">Required service for fetching image metadata.</param>
        /// <param name="allocationRateTracker">Required service for tracking the memory allocation rate.</param>
        /// <param name="configuration">Required configuration for the bot.</param>
        public InfoCommand(ImageUtilities imageUtilitiesService, AllocationRateTracker allocationRateTracker, TomoeConfiguration configuration)
        {
            _imageUtilitiesService = imageUtilitiesService;
            _allocationRateTracker = allocationRateTracker;
            _configuration = configuration;
        }
    }
}
