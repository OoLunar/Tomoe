namespace Tomoe.Utils.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class HierarchyException : Exception
    {
        public HierarchyException() { }
        public HierarchyException(string message) : base(message) { }
        public HierarchyException(string message, Exception inner) : base(message, inner) { }
        protected HierarchyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
