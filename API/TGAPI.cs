using Buratino.Helpers;
using Buratino.Xtensions;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
                //return OnUpdateWrapper(update);
            }
            catch (Exception e)
            {
                var chat = update.Message?.Chat?.Id ?? update.CallbackQuery?.Message?.Chat?.Id ?? 0;
                if (chat > 0)
                    botClient.SendOrUpdateMessage(chat, $"{e.Message}");
            }
            return Task.CompletedTask;
        }

        private Task HandlePollingError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        public string SendOrEdit(long chatId, string text, int msgId = default, InlineKeyboardConstructor inlineKeyboard = null, ParseMode? parseMode = ParseMode.Markdown)
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
                return Send(chatId, text, parseMode, inlineKeyboard);
            else
                return Edit(chatId, msgId, text, parseMode, inlineKeyboard);
        }

        private string Send(long chatId, string text, ParseMode? parseMode, InlineKeyboardConstructor inlineKeyboard = null)
        {
            return client.SendTextMessageAsync(chatId, text, parseMode, null, null, null, null, null, null, inlineKeyboard?.GetMarkup())
                .GetAwaiter().GetResult().MessageId.ToString();
        }

        private string Edit(long chatId, int messageId, string text, ParseMode? parseMode, InlineKeyboardConstructor inlineKeyboard = null)
        {
            return client.EditMessageTextAsync(chatId, messageId, text, parseMode, null, null, inlineKeyboard?.GetMarkup())
                .GetAwaiter().GetResult().MessageId.ToString();
        }

        public string Delete(long chatId, int messageId)
        {
            throw new NotImplementedException();
        }

        private string ReplaceMessages(long chatId, int messageId, string text, ParseMode? parseMode, InlineKeyboardConstructor inlineKeyboard = null)
        {
            Delete(chatId, messageId);
            return SendOrEdit(chatId, text, default, inlineKeyboard, parseMode);
        }

        private string SendFile(object chatId, string imageId, string caption, InlineKeyboardConstructor inlineKeyboard = null)
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
