namespace CompressionForce.Services.Exceptions
{
    /// <summary>
    /// Thrown when a use case rule is violated.
    /// </summary>
    public class ServiceException : Exception
    {
        public ServiceException(string message)
            : base(message)
        {
        }
    }
}
