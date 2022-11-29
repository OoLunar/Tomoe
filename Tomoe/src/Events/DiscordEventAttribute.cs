using System;

namespace OoLunar.Tomoe.Events
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class DiscordEventAttribute : Attribute { }
}
