﻿using Bronya.Caching.Structure;
using Bronya.DI;
using Bronya.Dtos;
using Bronya.Helpers;
using Bronya.Services;
using Bronya.Xtensions;

using System.Security.Cryptography;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bronya.API
{
    public class TGAPI
    {
        public TelegramBotClient client;
        private LogToFileService logService;

        public bool IsWorking { get; private set; }

        public ICacheService<StreamFileIdDto> StreamFilesCacheService { get; private set; }

        public delegate Task APIUpdateEventHandler(object sender, Update update);

        public event APIUpdateEventHandler UpdateEvent;

        public TGAPI(LogToFileService logService, string token)
        {
            Console.WriteLine(token);
            client = new TelegramBotClient(token, new HttpClient());
            StreamFilesCacheService = Container.Get<ICacheService<StreamFileIdDto>>();
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
            ConsoleXtensions.ColoredPrint("Request", ConsoleColor.Green);
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
                else if(sumText.Contains("message can't be deleted for everyone"))
                {

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
            ConsoleXtensions.ColoredPrint("End", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        private Task HandlePollingError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            logService.Log(exception);
            return Task.CompletedTask;
        }

        public string SendOrEdit(DataPackage package, string text, IReplyConstructor replyConstructor = null, ParseMode? parseMode = default, TGInputImplict file = default)
        {
            if (parseMode == default)
                parseMode = ParseMode.MarkdownV2;

            if (file == default)
            {
                if (package.MessageId == default)
                    return Send(package.ChatId, text, parseMode, replyConstructor);
                else
                {
                    if (package?.Update?.Type == UpdateType.CallbackQuery)
                    {
                        if (package?.Update?.CallbackQuery?.Message?.Type != MessageType.Text)
                        {
                            var msgId = Send(package.ChatId, text, parseMode, replyConstructor);
                            Delete(package.ChatId, package.MessageId);
                            return msgId;
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
                    return SendFile(package.ChatId, file, text, parseMode, replyConstructor);
                else
                {
                    if (package?.Update?.Type == UpdateType.CallbackQuery)
                    {
                        if (package?.Update?.CallbackQuery?.Message?.Type == MessageType.Photo)
                        {
                            return EditFile(package.ChatId, package.MessageId, file, text, parseMode, replyConstructor);
                        }
                        else
                        {
                            var msgId = SendFile(package.ChatId, file, text, parseMode, replyConstructor);
                            Delete(package.ChatId, package.MessageId);
                            return msgId;
                        }
                    }
                    return EditFile(package.ChatId, package.MessageId, file, text, parseMode, replyConstructor);
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
            if (replyConstructor is not InlineKeyboardConstructor inlineKeyboardConstructor)
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

        private string SendFile(long chatId, TGInputImplict image, string caption, ParseMode? parseMode, IReplyConstructor replyConstructor = null)
        {
            string hash = GetHash(image);

            var result = image.MediaType switch
            {
                InputMediaType.Photo => client.SendPhotoAsync(chatId, image.GetInputMedia(StreamFilesCacheService, hash), caption, parseMode, null, null, null, null, null, replyConstructor?.GetMarkup() ?? new ReplyKeyboardRemove())
                    .GetAwaiter().GetResult(),
                InputMediaType.Document => client.SendDocumentAsync(chatId, image.GetInputMedia(StreamFilesCacheService, hash), null, caption, parseMode, null, null, null, null, null, null, replyConstructor?.GetMarkup() ?? new ReplyKeyboardRemove())
                    .GetAwaiter().GetResult(),
                _ => throw new NotImplementedException("Не получилось отправить такое сообщение")
            };

            if (image.FileId == default && hash != default)
            {
                StreamFilesCacheService.Set(hash, new StreamFileIdDto(result.Photo.First().FileId));
            }
            return result.MessageId.ToString();
        }

        private string EditFile(long chatId, int messageId, TGInputImplict image, string caption, ParseMode? parseMode, IReplyConstructor replyConstructor = null)
        {
            if (replyConstructor is not InlineKeyboardConstructor inlineKeyboardConstructor)
                throw new InvalidOperationException("Нельзя передать кнопки сообщения. Нужно передать кнопки клавиатуры");

            string hash = GetHash(image);

            InputMediaBase media = image.MediaType switch
            {
                InputMediaType.Photo => new InputMediaPhoto(image.GetInputMedia(StreamFilesCacheService, hash)) { Caption = caption, ParseMode = parseMode },
                InputMediaType.Document => new InputMediaDocument(image.GetInputMedia(StreamFilesCacheService, hash)) { Caption = caption, ParseMode = parseMode },
                _ => throw new NotImplementedException()
            };

            var result = client.EditMessageMediaAsync(chatId, messageId, media, inlineKeyboardConstructor?.GetMarkup() as InlineKeyboardMarkup)
                .GetAwaiter().GetResult();

            if (image.FileId == default && hash != default)
            {
                StreamFilesCacheService.Set(hash, new StreamFileIdDto(result.Photo.First().FileId));
            }
            return result.MessageId.ToString();
        }

        public void AnswerCallbackQuery(object queryId, string text = null)
        {
            throw new NotImplementedException();
        }

        public void SendActions(object chatId, params ChatAction[] actions)
        {
            throw new NotImplementedException();
        }

        private static string GetHash(TGInputImplict image)
        {
            HashAlgorithm hasher = SHA512.Create();
            var hash = image.Stream != default
                ? hasher.ComputeHash(image.Stream).ToHex(true)
                : default;
            return hash;
        }
    }
}
