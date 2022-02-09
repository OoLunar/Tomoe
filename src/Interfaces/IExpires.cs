using System;

namespace Tomoe.Interfaces
{
    public interface IExpires
    {
        DateTime ExpiresAt { get; }
    }
}