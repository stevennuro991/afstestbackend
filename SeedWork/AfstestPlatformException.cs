namespace Afstest.API.SeedWork
{

    public class AfstestPlatformException : Exception
    {
        public int? CustomStatusCode { get; set; }
        public IEnumerable<string> Errors { get; private set; } = Enumerable.Empty<string>();

        public AfstestPlatformException() { }

        public AfstestPlatformException(string message) : base(message)
        {
            Errors = new string[] { message };
        }
        public AfstestPlatformException(IEnumerable<string> errors)
        {
            Errors = errors;
        }

        public AfstestPlatformException(string message, Exception innerException) : base(message, innerException) { }
    }
}
