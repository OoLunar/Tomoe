using System.ComponentModel.DataAnnotations;

namespace Tomoe.Models
{
	public class MenuRoleModel
	{
		[Key]
		public int Id { get; init; }
		public string ButtonId { get; init; } = null!;
		public ulong GuildId { get; init; }
		public ulong RoleId { get; init; }
	}
}
