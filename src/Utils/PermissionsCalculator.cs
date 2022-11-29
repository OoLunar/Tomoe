using System;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Tomoe.Utils
{
	public static class PermissionsCalculator
	{
		/// <summary>
		/// Checks to see if memberA can execute X action on memberB. Runs the following checks: If memberA is the guild owner, if memberB is the guild owner, if memberA has the proper permissions to execute X action, if memberB's hierarchy isn't higher than memberA's hierarchy.
		/// </summary>
		/// <param name="memberA">Which user is executing the action.</param>
		/// <param name="permissions">Which permission is associated with the action.</param>
		/// <param name="memberB">Who's being affected.</param>
		/// <returns>Whether memberA can execute X action on memberB.</returns>
		public static bool CanExecute(this DiscordMember memberA, Permissions permissions, DiscordMember memberB)
		{
			ArgumentNullException.ThrowIfNull(memberA, nameof(memberA));
			ArgumentNullException.ThrowIfNull(memberB, nameof(memberB));
			return memberA.IsOwner || (!memberB.IsOwner && memberA.Permissions.HasPermission(permissions) && memberB.Hierarchy < memberA.Hierarchy);
		}

		public static bool CanExecute(this IEnumerable<DiscordRole> roleListA, Permissions permissions, IEnumerable<DiscordRole> roleListB)
		{
			ArgumentNullException.ThrowIfNull(roleListA, nameof(roleListA));
			ArgumentNullException.ThrowIfNull(roleListB, nameof(roleListB));

			foreach (DiscordRole roleA in roleListA)
			{
				foreach (DiscordRole roleB in roleListB)
				{
					if (roleA.Permissions.HasPermission(permissions) && roleB.Position < roleA.Position)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
