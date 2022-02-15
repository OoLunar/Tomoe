using System;

namespace Tomoe.Interfaces
{
    public interface IExpires<T>
    {
        T Id { get; }
        DateTime ExpiresAt { get; }
    }
}