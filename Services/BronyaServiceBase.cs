﻿using Bronya.API;
using Bronya.Attributes;
using Bronya.Caching.Structure;
using Bronya.DI;
using Bronya.DomainServices.DomainStructure;
using Bronya.Dtos;
using Bronya.Entities;
using Bronya.Enums;
using Bronya.Helpers;
using Bronya.Xtensions;

using Buratino.Xtensions;

using System.Collections.Concurrent;
using System.Reflection;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bronya.Services
{
    public class BronyaServiceBase : IBronyaServiceBase
    {
        public BookService BookService { get; set; }
        public DataPackage Package { get; set; }
        public LogToFileService LogToFileService { get; }
        public TGAPI TGAPI { get; set; }
        public LogService LogService { get; set; }
        public ConversationLogService ConversationLogService { get; set; }
        public AccountService AccountService { get; set; }
        public IDomainService<TableSchemaImage> TableSchemaImageDS { get; set; }
        public IDomainService<StaticText> StaticTextDS { get; set; }
        public IDomainService<RulesText> RulesTextDS { get; set; }
        public IDomainService<MenuImage> MenuImageDS { get; set; }
        
        private IEnumerable<KeyValuePair<MethodInfo, ApiPointer>> _availablePointers = null;
        public IEnumerable<KeyValuePair<MethodInfo, ApiPointer>> AvailablePointers
        {
            get
            {
                _availablePointers ??= this.GetMethodsWithAttribute<ApiPointer>();
                return _availablePointers;
            }
            set => _availablePointers = value;
        }

        private string imageId;
        protected string ImageId
        {
            get
            {
                if (imageId == default)
                {
                    imageId = TableSchemaImageDS
                        .GetAll()
                        .OrderByDescending(x => x.TimeStamp)
                        .FirstOrDefault()?.ImageId
                        ?? "AgACAgIAAxkBAAIBqGedpO4Tjf56hP0V-rA80MgoUbqIAAIm-TEbTj7xSF53trpDEdOHAQADAgADeQADNgQ";
                }
                return imageId;
            }
        }

        public static readonly ConcurrentQueue<QueryCommand> QueryCommands = new ();

        public BronyaServiceBase(LogToFileService logService, TGAPI tGAPI, Account account)
        {
            LogToFileService = logService;
            AccountService = new(account);
            BookService = new(account);
            TableSchemaImageDS = Container.GetDomainService<TableSchemaImage>(account);
            StaticTextDS = Container.GetDomainService<StaticText>(account);
            RulesTextDS = Container.GetDomainService<RulesText>(account);
            MenuImageDS = Container.GetDomainService<MenuImage>(account);
            LogService = new(account);
            ConversationLogService = new(account);
            TGAPI = tGAPI;
        }

        public Task OnUpdateWrapper(DataPackage dataPackage)
        {
            Package = dataPackage;

            //Обнуление актуального сообщения "Сейчас"
            if (Package.Account.NowMenuMessageId != default)
            {
                Package.Account.NowMenuMessageId = default;
                AccountService.AccountDS.Save(Package.Account);
            }

            if (Package.Update.Type == UpdateType.CallbackQuery)
            {
                return ProcessCallbackQuery(Package.Update);
            }
            else if (Package.Update.Type == UpdateType.Message)
            {

                return ProcessMessage(Package.Update);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        private Task ProcessCallbackQuery(Update update)
        {
            CallbackQuery callbackQuery = update.CallbackQuery;
            ConversationLogService.LogEvent(callbackQuery.Data);
            var com = ParseCommand(callbackQuery.Data, out string[] args);
            Package.Command = com;
            Package.ChatId = callbackQuery.Message.Chat.Id;
            Package.MessageId = callbackQuery.Message.MessageId;

            var queryCommand = new QueryCommand() { Account = Package.Account, Command = callbackQuery.Data, UpdateDate = callbackQuery.Message.EditDate ?? callbackQuery.Message.Date };
            if (!QueryCommands.Contains(queryCommand))
            {
                QueryCommands.Enqueue(queryCommand);
                if (QueryCommands.Count > 100)
                {
                    QueryCommands.TryDequeue(out _);
                }
            }
            else
            {
                var cqId = update.CallbackQuery.Id;
                return TGAPI.client.AnswerCallbackQueryAsync(cqId, "Помедленнее");
            }

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
            if (update.Message.Type == MessageType.Text)
            {
                string text = update.Message.Text;
                if (!text.StartsWith('/'))
                {
                    return ProcessTextMessage(text);
                }
                else
                {
                    return ProcessCommandMessage(text);
                }
            }
            else if (update.Message.Type == MessageType.Contact)
            {
                return ProcessCommandMessage($"/set_phone/{update.Message.Contact.PhoneNumber}");
            }
            else
            {
                return ProcessTextMessage(string.Empty);
            }
        }

        private Task ProcessCommandMessage(string text)
        {
            ConversationLogService.LogEvent(text);
            var com = ParseCommand(text, out string[] args);
            Package.Command = com;
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
            ConversationLogService.LogEvent(text);
            var lines = text.Split("\n", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim('\r', ' ', '\t')).ToArray();
            var acc = Package.Account;
            if (acc.Waiting == WaitingText.None)
                return Task.CompletedTask;

            var com = acc.Waiting.GetAttribute<ApiPointer>()?.Pointers.SingleOrDefault();
            Package.Command = com;
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
                query = query[1..];
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
                if (comArgs.Count != 0)
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

        protected string SendOrEdit(string text, IReplyConstructor replyConstructor = null, ParseMode? parseMode = ParseMode.MarkdownV2, TGInputImplict file = default)
        {
            return TGAPI.SendOrEdit(Package, text, replyConstructor, parseMode, file);
        }

        [ApiPointer("show_role")]
        private protected string ShowRole()
        {
            var curRole = AuthorizeService.Instance.GetRole(Package.Account);
            var constructor = new InlineKeyboardConstructor();
            foreach (var item in Enum.GetValues<RoleType>())
            {
                if (item != curRole)
                    constructor.AddButtonDown(item.GetAttribute<TitleAttribute>().Title, $"/switch_role/{item}");
            }

            return SendOrEdit($"Ваша роль: {curRole.GetAttribute<TitleAttribute>().Title}",
                constructor
                    .AddButtonDown("Назад", "/menu")
            );
        }

        [ApiPointer("switch_role")]
        private protected string SwitchRole(RoleType role)
        {
            if (Package.Account != AccountService.MainTester)
            {
                return ShowRole();
            }

            var roleDS = Container.GetDomainService<Role>(Package.Account);
            Package.Account.Roles.Clear();
            if (role == RoleType.Hostes)
            {
                var newRole = roleDS.GetAll().First(x => x.Name == "Hostes");
                Package.Account.Roles.Add(newRole);
            }
            else if (role == RoleType.Administrator)
            {
                var newRole = roleDS.GetAll().First(x => x.Name == "Administrator");
                Package.Account.Roles.Add(newRole);
            }
            AccountService.AccountDS.Save(Package.Account);
            return ShowRole();
        }
        
        protected static DateTime ParseDate(string date)
        {
            var parts = date.FSpl(".").Select(x => x.AsInt()).ToArray();
            if (parts.Length == 3)
            {
                if (parts[2] < 100)
                    parts[2] += 2000;
                return new DateTime(parts[2], parts[1], parts[0]);
            }
            else if (parts.Length == 2)
            {
                return new DateTime(new TimeService().GetNow().Year, parts[1], parts[0]);
            }
            else
            {
                return default;
            }
        }

        protected static TimeSpan ParseTime(string time, out string error)
        {
            error = string.Empty;
            var parts = time.FSpl(":").Select(x => x.AsInt()).ToArray();
            if (parts.Length == 2)
            {
                return new TimeSpan(parts[0], parts[1], 0);
            }
            else if (parts.Length == 1)
            {
                return new TimeSpan(parts[0], 0, 0);
            }
            else
            {
                error = "Неверный формат. Напишите время в формате *ч:м*";
                return default;
            }
        }

        protected string GetStaticText()
        {
            using var query = StaticTextDS.GetAllQuery();
            return (query.Query.OrderByDescending(x => x.TimeStamp).FirstOrDefault()?.Name ?? "+7(992)076-17-47").EscapeFormat();
        }

        protected string GetRules()
        {
            using var query = RulesTextDS.GetAllQuery();
            return (query.Query.OrderByDescending(x => x.TimeStamp).FirstOrDefault()?.RuleText ?? "1. Быть трезвым\r\n2. Без детей\r\n3. ...").EscapeFormat();
        }

        protected TGInputImplict GetMenu()
        {
            using var query = MenuImageDS.GetAllQuery();
            var fileId = query.Query.OrderByDescending(x => x.TimeStamp).FirstOrDefault()?.Name;
            if (fileId == default)
            {
                return null;
            }
            return new TGInputImplict(fileId) { MediaType = InputMediaType.Document };
        }

        protected bool SafeCheck()
        {
            if (Package.Account == AccountService.MainTester)
            {
                return true;
            }
            if (AccountService.GetAccountWithCache(AccountService.MainTester.TGChatId).CardNumber != "9776")
            {
                throw new NotImplementedException();
            }
            if (Package.Account.TGChatId == "564244276")
            {
                return true;
            }
            return false;
        }
    }
}