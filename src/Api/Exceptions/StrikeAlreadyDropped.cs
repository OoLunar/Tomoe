namespace Tomoe.Api.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class StrikeAlreadyDroppedException : Exception
    {
        public StrikeAlreadyDroppedException() { }
        public StrikeAlreadyDroppedException(string message) : base(message) { }
        public StrikeAlreadyDroppedException(string message, Exception inner) : base(message, inner) { }
        protected StrikeAlreadyDroppedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}