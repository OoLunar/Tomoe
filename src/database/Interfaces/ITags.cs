using System;

namespace Tomoe.Database.Interfaces
{
	public interface ITags : IDisposable
	{
		string? Retrieve(ulong guildId, string tagTitle);
		string[] GetGuild(ulong guildId);
		string[] GetUser(ulong guildId, ulong userId);
		string[] GetUser(ulong userId);
		string[] GetAliases(ulong guildId, string tagTitle);
		void Delete(ulong guildId, string tagTitle);
		void DeleteAlias(ulong guildId, string tagTitle);
		void DeleteAllAliases(ulong guildId, string tagTitle);
		void Edit(ulong guildId, string tagTitle, string content);
		void Create(ulong guildId, ulong userId, string tagTitle, string content);
		void CreateAlias(ulong guildId, ulong userId, string tagTitle, string oldTagTitle);
		ulong? GetAuthor(ulong guildId, string tagTitle);
		bool? IsAlias(ulong guildId, string tagTitle);
		bool Exist(ulong guildId, string tagTitle);
		void Claim(ulong guildId, string tagTitle, ulong newAuthor);
		string RealName(ulong guildId, string tagTitle);
	}
}
