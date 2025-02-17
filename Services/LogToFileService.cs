using Bronya.Xtensions;

using System.Text;

namespace Bronya.Services
{
    /// <summary>
    /// Логирование ошибок в файл
    /// </summary>
    public class LogToFileService : IDisposable
    {
        public StreamWriter LogWriter { get; set; } = new StreamWriter($"logs/log_{DateTime.Now:d HH}.txt", Encoding.UTF8, new FileStreamOptions()
        {
            Access = FileAccess.Write,
            Share = FileShare.Read,
            Mode = FileMode.Append,
        });

        public void Dispose()
        {
            LogWriter.Close();
            LogWriter.Dispose();
        }

        ~LogToFileService()
        {
            Dispose();
        }

        public void Log(string message)
        {
            LogWriter.WriteLine(message.Replace("***", "%*%*%*") + "\r\n***"); //todo - файл остается пустым
        }

        public void Log(Exception exception)
        {
            Log(exception.CollectMessagesFromException() +
                "\r\n\r\n[Stack trace]:" +
                $"\r\n{exception.StackTrace}");
        }
    }
}
