namespace Bronya.Xtensions
{
    public static class ExceptionExtensions
    {
        public static string CollectMessagesFromException(this Exception exception)
        {
            if (exception.InnerException is null)
                return $"{exception.GetType().Name}: {exception.Message}";
            else
                return $"{CollectMessagesFromException(exception.InnerException)}" +
                    $"\r\n\r\n{exception.GetType().Name}: {exception.Message}";
        }

        public static string CollectStackTracesFromException(this Exception exception)
        {
            if (exception.InnerException is null)
                return $"{exception.GetType().Name}: {exception.StackTrace}";
            else
                return $"{CollectStackTracesFromException(exception.InnerException)}" +
                    $"\r\n\r\n{exception.GetType().Name}: {exception.StackTrace}";
        }
    }
}
