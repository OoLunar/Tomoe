using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Shows general information about whatever the user wants.
    /// </summary>
    [Command("info"), RequirePermissions(DiscordPermissions.EmbedLinks, DiscordPermissions.None)]
    public sealed partial class InfoCommand
    {
        /// <summary>
        /// Creates a new instance of <see cref="InfoCommand"/>.
        /// </summary>
        /// <param name="imageUtilitiesService">Required service for fetching image metadata.</param>
        /// <param name="allocationRateTracker">Required service for tracking the memory allocation rate.</param>
        public InfoCommand(ImageUtilities imageUtilitiesService, AllocationRateTracker allocationRateTracker)
        {
            _imageUtilitiesService = imageUtilitiesService;
            _allocationRateTracker = allocationRateTracker;
        }
    }
}
