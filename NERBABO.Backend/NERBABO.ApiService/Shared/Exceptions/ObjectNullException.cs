namespace NERBABO.ApiService.Shared.Exceptions
{
    public class ObjectNullException: Exception
    {
        public ObjectNullException(string message) : base(message) { }
        public ObjectNullException(string message, Exception innerException) : base(message, innerException) { }
    }
}
