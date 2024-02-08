using System;

namespace OoLunar.Tomoe.Database
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = true)]
    public sealed class DatabaseModelAttribute : Attribute;
}
