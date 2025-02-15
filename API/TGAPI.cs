using Bronya.Dtos;
using Bronya.Helpers;
using Bronya.Xtensions;

using Buratino.Helpers;
using Buratino.Xtensions;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

using vkteams.Services;

namespace Buratino.API
{
    public class TGAPI
    {
        public TelegramBotClient client;
        private LogToFileService logService;

        public bool IsWorking { get; private set; }

        public delegate Task APIUpdateEventHandler(object sender, Update update);

        public event APIUpdateEventHandler UpdateEvent;

        public TGAPI(LogToFileService logService, string token)
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

        public string SendOrEdit(DataPackage package, string text, IReplyConstructor replyConstructor = null, ParseMode? parseMode = ParseMode.Markdown, TGInputImplict file = null)
        {
            if (parseMode == default)
                parseMode = ParseMode.Markdown;

            if (parseMode == ParseMode.MarkdownV2)
            {
                text = string.Concat(text.ToArray().Select(x =>
                {
                    if (((int)x).Between_LTE_GTE(1, 126))
                        return $"\\{x}";
                    return x.ToString();
                }));
            }

            if (file == default)
            {
                if (package.MessageId == default)
                    return Send(package.ChatId, text, parseMode, replyConstructor);
                else
                {
                    if (package?.Update?.Type == UpdateType.CallbackQuery)
                    {
                        if (package?.Update?.CallbackQuery?.Message?.Type == MessageType.Photo)
                        {
                            Send(package.ChatId, text, parseMode, replyConstructor);
                            return Delete(package.ChatId, package.MessageId);
                        }
                        else
                        {

                            return Edit(package.ChatId, package.MessageId, text, parseMode, replyConstructor);
                        }
                    }
                    return Edit(package.ChatId, package.MessageId, text, parseMode, replyConstructor);
                }   
            }
            else
            {
                if (package.MessageId == default)
                    return SendFile(package.ChatId, file.GetInputOnlineFile(), text, parseMode, replyConstructor);
                else
                {
                    if (package?.Update?.Type == UpdateType.CallbackQuery)
                    {
                        if (package?.Update?.CallbackQuery?.Message?.Type == MessageType.Photo)
                        {
                            return EditFile(package.ChatId, package.MessageId, file.GetInputMedia(), text, parseMode, replyConstructor);
                        }
                        else
                        {
                            SendFile(package.ChatId, file.GetInputOnlineFile(), text, parseMode, replyConstructor);
                            return Delete(package.ChatId, package.MessageId);
                        }
                    }
                    return EditFile(package.ChatId, package.MessageId, file.GetInputMedia(), text, parseMode, replyConstructor);
                }
            }
        }

        private string Send(long chatId, string text, ParseMode? parseMode, IReplyConstructor replyConstructor = null)
        {
            return client.SendTextMessageAsync(chatId, text, parseMode, null, null, null, null, null, null, replyConstructor?.GetMarkup() ?? new ReplyKeyboardRemove())
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
            client.DeleteMessageAsync(chatId, messageId)
                .GetAwaiter().GetResult();
            return string.Empty;
        }

        private string SendFile(long chatId, InputOnlineFile image, string caption, ParseMode? parseMode, IReplyConstructor replyConstructor = null)
        {
            return client.SendPhotoAsync(chatId, image, caption, parseMode, null, null, null, null, null, replyConstructor?.GetMarkup() ?? new ReplyKeyboardRemove())
                .GetAwaiter().GetResult().MessageId.ToString();
        }
        
        private string EditFile(long chatId, int messageId, InputMedia image, string caption, ParseMode? parseMode, IReplyConstructor replyConstructor = null)
        {
            if (!(replyConstructor is InlineKeyboardConstructor inlineKeyboardConstructor))
                throw new InvalidOperationException("Нельзя передать кнопки сообщения. Нужно передать кнопки клавиатуры");

            return client.EditMessageMediaAsync(chatId, messageId, new InputMediaPhoto(image) { Caption = caption, ParseMode = parseMode }, inlineKeyboardConstructor?.GetMarkup() as InlineKeyboardMarkup)
                .GetAwaiter().GetResult().MessageId.ToString();
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
