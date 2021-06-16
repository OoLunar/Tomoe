namespace Tomoe.Api.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class StrikeNotFoundException : Exception
    {
        public StrikeNotFoundException() { }
        public StrikeNotFoundException(string message) : base(message) { }
        public StrikeNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected StrikeNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}