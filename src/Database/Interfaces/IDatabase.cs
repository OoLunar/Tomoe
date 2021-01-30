using System;

namespace Tomoe.Database.Interfaces
{
	public interface IDatabase : IDisposable
	{
		IUser User { get; }
		IGuild Guild { get; }
		//ITags Tags { get; }
		IAssignment Assignments { get; }
		IStrikes Strikes { get; }
	}
}
