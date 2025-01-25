using Bronya.Services;

using Buratino.API;
using Buratino.Attributes;
using Buratino.Entities;
using Buratino.Enums;
using Buratino.Xtensions;

using System.Reflection;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace vkteams.Services
{
    public class BronyaServiceBase
    {
        public LogService LogService { get; }
        public TGAPI TGAPI { get; set; }

        private IEnumerable<KeyValuePair<MethodInfo, TGPointerAttribute>> _availablePointers = null;
        protected AccountService AccountService;

        public BronyaServiceBase(LogService logService, TGAPI tgAPI)
        {
            LogService = logService;
            AccountService = new AccountService();
            TGAPI = tgAPI;
            TGAPI.UpdateEvent += OnUpdateWrapper;
            tgAPI.Start();
        }

        public IEnumerable<KeyValuePair<MethodInfo, TGPointerAttribute>> AvailablePointers
        {
            get
            {
                if (_availablePointers is null)
                {
                    _availablePointers = this.GetMethodsWithAttribute<TGPointerAttribute>();
                }
                return _availablePointers;
            }
            set => _availablePointers = value;
        }

        private Task OnUpdateWrapper(object sender, Update update)
        {
            var acc = AccountService.GetAccount(update);
            if (update.Type == UpdateType.Message)
            {
                return ProcessMessage(acc, update);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                return ProcessCallbackQuery(acc, update);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        private Task ProcessCallbackQuery(Account acc, Update update)
        {
            var com = ParseCommand(update.CallbackQuery.Data, out string[] args);
            var chat = update.CallbackQuery.Message.Chat.Id;
            var messageId = update.CallbackQuery.Message.MessageId;
            //client.AnswerCallbackQueryAsync(update.CallbackQuery.Id);

            var availablePointers = this.GetMethodsWithAttribute<TGPointerAttribute>();
            var method = availablePointers.SingleOrDefault(x => x.Value.Pointers.Contains(com));
            if (method.Key is not null)
            {
                InvokeCommand(method, chat, messageId, args, acc);
            }
            else
            {
                throw new Exception("Не поддерживаемая команда");
            }
            return Task.CompletedTask;
        }

        private Task ProcessMessage(Account acc, Update update)
        {
            var chat = update.Message.Chat.Id;
            string text = update.Message.Text;
            if (!text.StartsWith("/"))
            {
                return ProcessTextMessage(acc, chat, text);
            }
            else
            {
                return ProcessCommandMessage(acc, chat, text);
            }
        }

        private Task ProcessCommandMessage(Account acc, long chat, string text)
        {
            var com = ParseCommand(text, out string[] args);
            var availablePointers = this.GetMethodsWithAttribute<TGPointerAttribute>();
            var method = availablePointers.SingleOrDefault(x => x.Value.Pointers.Contains(com));
            if (method.Key is not null)
            {
                InvokeCommand(method, chat, 0, args, acc);
            }
            else
            {
                throw new Exception("Не поддерживаемая команда");
            }
            return Task.CompletedTask;
        }

        private Task ProcessTextMessage(Account acc, long chat, string text)
        {
            var lines = text.Split("\n", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim('\r', ' ', '\t')).ToArray();
            TGActionType actionType = TGActionType.AddCharge;
            //if (!ChatActionPointer.TryGetValue(chat, out TGActionType actionType))
            //{
            //    throw new ArgumentNullException(nameof(actionType));
            //}
            //ChatSourcePointer.TryGetValue(chat, out Guid id);//get from DB

            if (actionType == TGActionType.AddCharge)
            {
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
        private KeyValuePair<MethodInfo, TGPointerAttribute> GetMethod(string com)
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
        private string InvokeCommand(KeyValuePair<MethodInfo, TGPointerAttribute> method, object chat, object messageId, string[] args, Account acc = null, Update sourceEvent = null)
        {
            var parameters = method.Key.GetParameters();
            var arguments = new object[parameters.Length];
            var comArgs = new Queue<string>(args);
            for (int i = 0; i < parameters.Length; i++)
            {
                var item = parameters[i];
                if (item.Name == "chatId")
                    arguments[i] = chat;
                else if (item.Name == "messageId")
                    arguments[i] = messageId;
                else if (item.Name == "acc")
                    arguments[i] = acc;
                else if (item.Name == "source")
                    arguments[i] = sourceEvent;
                else if (comArgs.Any())
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
    }
}