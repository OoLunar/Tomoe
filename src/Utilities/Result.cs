namespace Tomoe.Utils
{
    public class Result<T>
    {
        public T Value { get; }
        public bool HasFailed { get; }
        public string FailReason { get; }

        public Result(T value) => Value = value;
        public Result(string failReason)
        {
            HasFailed = true;
            FailReason = failReason;
        }
    }
}