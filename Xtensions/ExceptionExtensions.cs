namespace Bronya.Xtensions
{
    public static class ExceptionExtensions
    {
        public static string CollectMessagesFromException(this Exception exception)
        {
            if (exception.InnerException is null)
                return $"{exception.GetType().Name}: {exception.Message}";
            else
                return $"{exception.GetType().Name}: {exception.Message}\r\n\r\n" + CollectMessagesFromException(exception.InnerException);
        }
    }
}
