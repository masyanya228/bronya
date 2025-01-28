using Bronya.Xtensions;

using Buratino.Helpers;
using Buratino.Xtensions;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using vkteams.Services;

namespace Buratino.API
{
    public class TGAPI
    {
        public TelegramBotClient client;
        private LogService logService;

        public bool IsWorking { get; private set; }

        public delegate Task APIUpdateEventHandler(object sender, Update update);

        public event APIUpdateEventHandler UpdateEvent;

        public TGAPI(LogService logService, string token)
        {
            client = new TelegramBotClient(token, new HttpClient());
            this.logService = logService;
            IsWorking = true;
        }

        public void Start()
        {
            var me = client.GetMeAsync().GetAwaiter().GetResult();
            client.StartReceiving(OnUpdate, HandlePollingError);
        }

        private Task OnUpdate(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            try
            {
                UpdateEvent.Invoke(this, update);
            }
            catch (Exception e)
            {
                logService.Log(e);
                string sumText = e.CollectMessagesFromException();
                if (sumText.Contains("message is not modified"))
                {
                    if (update.Type == UpdateType.CallbackQuery)
                    {
                        var cqId = update.CallbackQuery.Id;
                        botClient.AnswerCallbackQueryAsync(cqId, "Помедленнее");
                    }
                }
                else
                {
                    if (update.Type == UpdateType.Message)
                    {
                        var chat = update.Message.Chat.Id;
                        botClient.SendOrUpdateMessage(chat, $"{e.Message}");
                    }
                    else if (update.Type == UpdateType.CallbackQuery)
                    {
                        var chat = update.CallbackQuery.Message.Chat.Id;
                        botClient.SendOrUpdateMessage(chat, $"{e.Message}");
                    }
                }
            }
            return Task.CompletedTask;
        }

        private Task HandlePollingError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            logService.Log(exception);
            return Task.CompletedTask;
        }

        public string SendOrEdit(long chatId, string text, int msgId = default, IReplyConstructor replyConstructor = null, ParseMode? parseMode = ParseMode.Markdown)
        {
            if (parseMode == ParseMode.MarkdownV2)
            {
                text = string.Concat(text.ToArray().Select(x =>
                {
                    if (((int)x).Between_LTE_GTE(1, 126))
                        return $"\\{x}";
                    return x.ToString();
                }));
            }
            if (msgId == default)
                return Send(chatId, text, parseMode, replyConstructor);
            else
                return Edit(chatId, msgId, text, parseMode, replyConstructor);
        }

        private string Send(long chatId, string text, ParseMode? parseMode, IReplyConstructor replyConstructor = null)
        {
            return client.SendTextMessageAsync(chatId, text, parseMode, null, null, null, null, null, null, replyConstructor?.GetMarkup())
                .GetAwaiter().GetResult().MessageId.ToString();
        }

        private string Edit(long chatId, int messageId, string text, ParseMode? parseMode, IReplyConstructor replyConstructor = null)
        {
            if (!(replyConstructor is InlineKeyboardConstructor inlineKeyboardConstructor))
                throw new InvalidOperationException("Нельзя передать кнопки сообщения. Нужно передать кнопки клавиатуры");
            return client.EditMessageTextAsync(chatId, messageId, text, parseMode, null, null, inlineKeyboardConstructor?.GetMarkup() as InlineKeyboardMarkup)
                .GetAwaiter().GetResult().MessageId.ToString();
        }

        public string Delete(long chatId, int messageId)
        {
            throw new NotImplementedException();
        }

        private string ReplaceMessages(long chatId, int messageId, string text, ParseMode? parseMode, IReplyConstructor replyConstructor = null)
        {
            Delete(chatId, messageId);
            return SendOrEdit(chatId, text, default, replyConstructor, parseMode);
        }

        private string SendFile(object chatId, string imageId, string caption, IReplyConstructor replyConstructor = null)
        {
            throw new NotImplementedException();
        }

        public void AnswerCallbackQuery(object queryId, string text = null)
        {
            throw new NotImplementedException();
        }

        public void SendActions(object chatId, params ChatAction[] actions)
        {
            throw new NotImplementedException();
        }
    }
}
