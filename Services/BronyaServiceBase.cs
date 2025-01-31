using Bronya.Dtos;
using Bronya.Services;

using Buratino.API;
using Buratino.Attributes;
using Buratino.Enums;
using Buratino.Helpers;
using Buratino.Xtensions;

using System.Reflection;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace vkteams.Services
{
    public class BronyaServiceBase : IBronyaServiceBase
    {
        public DataPackage Package {  get; set; }
        public LogService LogService { get; }
        public TGAPI TGAPI { get; set; }

        private IEnumerable<KeyValuePair<MethodInfo, ApiPointer>> _availablePointers = null;
        public AccountService AccountService {  get; set; }

        public BronyaServiceBase(LogService logService, TGAPI tGAPI)
        {
            LogService = logService;
            AccountService = new AccountService();
            TGAPI = tGAPI;
        }

        public IEnumerable<KeyValuePair<MethodInfo, ApiPointer>> AvailablePointers
        {
            get
            {
                if (_availablePointers is null)
                {
                    _availablePointers = this.GetMethodsWithAttribute<ApiPointer>();
                }
                return _availablePointers;
            }
            set => _availablePointers = value;
        }

        public Task OnUpdateWrapper(DataPackage dataPackage)
        {
            Package = dataPackage;
            if (Package.Update.Type == UpdateType.CallbackQuery)
            {
                return ProcessCallbackQuery(Package.Update);
            }
            else if (Package.Update.Type == UpdateType.Message)
            {
                if (Package.Update.Message.Type == MessageType.Photo)
                {
                    var fileId = Package.Update.Message.Photo.Last().FileId;
                    return Task.CompletedTask;
                }
                else
                {
                    return ProcessMessage(Package.Update);
                }
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        private Task ProcessCallbackQuery(Update update)
        {
            var com = ParseCommand(update.CallbackQuery.Data, out string[] args);
            Package.ChatId = update.CallbackQuery.Message.Chat.Id;
            Package.MessageId = update.CallbackQuery.Message.MessageId;

            var method = GetMethod(com);
            if (method.Key is not null)
            {
                InvokeCommand(method, args);
            }
            else
            {
                throw new Exception("Не поддерживаемая команда");
            }
            return Task.CompletedTask;
        }

        private Task ProcessMessage(Update update)
        {
            Package.ChatId = update.Message.Chat.Id;
            string text = update.Message.Text;
            if (!text.StartsWith("/"))
            {
                return ProcessTextMessage(text);
            }
            else
            {
                return ProcessCommandMessage(text);
            }
        }

        private Task ProcessCommandMessage(string text)
        {
            var com = ParseCommand(text, out string[] args);
            var method = GetMethod(com);
            if (method.Key is not null)
            {
                InvokeCommand(method, args);
            }
            else
            {
                throw new Exception("Не поддерживаемая команда");
            }
            return Task.CompletedTask;
        }

        private Task ProcessTextMessage(string text)
        {
            var lines = text.Split("\n", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim('\r', ' ', '\t')).ToArray();
            var acc = Package.Account;
            if (acc.Waiting == WaitingText.None)
                return Task.CompletedTask;

            var com = acc.Waiting.GetAttribute<ApiPointer>()?.Pointers.SingleOrDefault();
            var method = GetMethod(com);
            if (method.Key is not null)
            {
                InvokeCommand(method, lines);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Разобрать запрос на команду и аргументы
        /// </summary>
        /// <param name="query"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private string ParseCommand(string query, out string[] args)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException($"\"{nameof(query)}\" не может быть пустым или содержать только пробел.", nameof(query));
            }
            if (query.StartsWith('/'))
            {
                query = query.Substring(1);
                args = query.Split('/').Skip(1).ToArray();
                return query.Split('/').First().ToLower();
            }
            else
            {
                args = query.Split("/").ToArray();
                return null;
            }
        }

        /// <summary>
        /// Получить метод
        /// </summary>
        /// <param name="com"></param>
        /// <returns></returns>
        private KeyValuePair<MethodInfo, ApiPointer> GetMethod(string com)
        {
            var method = AvailablePointers.SingleOrDefault(x => x.Value.Pointers.Contains(com));
            return method;
        }

        /// <summary>
        /// Подготовить аргументы и вызвать метод
        /// </summary>
        /// <param name="method"></param>
        /// <param name="chat"></param>
        /// <param name="messageId"></param>
        /// <param name="args"></param>
        /// <param name="person"></param>
        /// <param name="sourceEvent"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private string InvokeCommand(KeyValuePair<MethodInfo, ApiPointer> method, string[] args)
        {
            var parameters = method.Key.GetParameters();
            var arguments = new object[parameters.Length];
            var comArgs = new Queue<string>(args);
            for (int i = 0; i < parameters.Length; i++)
            {
                var item = parameters[i];
                if (comArgs.Any())
                {
                    arguments[i] = comArgs.Dequeue().Cast(item.ParameterType);
                }
                else if (item.IsOptional)
                {
                    arguments[i] = item.DefaultValue;
                }
                else
                {
                    throw new ArgumentException("Не хватает аргументов для вызова метода");
                }
            }
            return method.Key.Invoke(this, arguments).ToString();
        }

        protected string SendOrEdit(string text, IReplyConstructor replyConstructor = null, ParseMode? parseMode = ParseMode.Markdown, string imageId = default)
        {
            return TGAPI.SendOrEdit(Package, text, replyConstructor, parseMode, imageId);
        }
    }
}