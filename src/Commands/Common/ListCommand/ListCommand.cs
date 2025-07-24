using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees.Metadata;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Manages lists for the user, allowing them to create, view, and manage items within those lists.
    /// </summary>
    /// <remarks>
    /// Intended to be used as a todo list or as a shopping list.
    /// </remarks>
    [Command("list"), TextAlias("shop", "todo")]
    public static partial class ListCommand;
}