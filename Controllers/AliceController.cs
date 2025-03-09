using Bronya.DomainServices.DomainStructure;
using Bronya.Entities;
using Bronya.Enums;
using Bronya.Services;
using Bronya.Xtensions;

using Buratino.Xtensions;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace Bronya.Controllers
{
    public class AliceController : Controller
    {
        public IDomainService<AliceDialog> AliceDialogDS { get; set; }

        private static string[] YesPhrases = ["да", "давай", "поехали", "погнали", "хорошо", "попробуем", "допустим"];
        private static string[] NoPhrases = ["нет", "неправильно"];
        private static string[] ResetPhrases = ["отмена", "заново", "передумал"];

        public AliceController(IDomainService<AliceDialog> aliceDialogDS)
        {
            AliceDialogDS = aliceDialogDS;
        }

        [HttpPost]
        public async Task<AliceResponse> Hook()
        {
            StreamReader sr = new StreamReader(HttpContext.Request.Body);
            var json = await sr.ReadToEndAsync();
            AliceRequest request = JsonConvert.DeserializeObject<AliceRequest>(json);
            try
            {
                return DialogPath(request);
            }
            catch (Exception ex)
            {
                if (request.Request.Type == RequestType.SimpleUtterance)
                {
                    return request.Reply("Что-то пошло не так. Позвоните, пожалуйста по телефону.");
                }
                else
                {
                    return request.Reply("Что-то пошло не так. Позвоните, пожалуйста по телефону +7(992)076-17-47.");
                }
            }
        }

        private AliceResponse DialogPath(AliceRequest request)
        {
            var dialogSession = AliceDialogDS.GetAll(x => x.SessionId == request.Session.SessionId).FirstOrDefault();
            if (request.Session.New || dialogSession == default)
            {
                var oldUser = AliceDialogDS.GetAll(x => x.UserId == request.Session.UserId)
                    .OrderByDescending(x => x.TimeStamp)
                    .FirstOrDefault(x => x.Name != default);
                dialogSession = AliceDialogDS.Save(new AliceDialog()
                {
                    State = AliceDialogState.AskIntent,
                    SessionId = request.Session.SessionId,
                    UserId = request.Session.UserId,
                    Name = oldUser.Name
                });

                if (oldUser != default)
                {
                    dialogSession.State = AliceDialogState.AskIntent;
                    AliceDialogDS.Save(dialogSession);
                    return request.Reply($"Привет, {oldUser.Name}! Заброн+ируем столик?",
                        false,
                        [
                            new ButtonModel() { Title = "Да" },
                            new ButtonModel() { Title = "Нет" },
                            new ButtonModel() { Title = "Что ты умеешь?" },
                        ]);
                }
                else
                {
                    dialogSession.State = AliceDialogState.AskName;
                    AliceDialogDS.Save(dialogSession);
                    return request.Reply("Привет! Я помогу заброн+ировать стол в The Green Place. Как я могу к вам обращаться?");
                }
            }
            else
            {
                ArgumentNullException.ThrowIfNull(dialogSession);
                if (request.Request.OriginalUtterance.FSpl(" ").Intersect(ResetPhrases).Any())
                {
                    dialogSession.State = AliceDialogState.AskName;
                    AliceDialogDS.Save(dialogSession);
                    return request.Reply("Как я могу к вам обращаться?");
                }

                if (dialogSession.State == AliceDialogState.None)
                {
                    if (request.Request.Type == RequestType.SimpleUtterance)
                    {
                        if (request.Request.OriginalUtterance == "забронировать стол")
                        {
                            dialogSession.State = AliceDialogState.AskSeatAmount;
                            AliceDialogDS.Save(dialogSession);
                            return request.Reply("Сколько будет гостей?");
                        }
                        else if (request.Request.OriginalUtterance.FSpl(" ").Any(x => x == "помощь"))
                        {
                            dialogSession.State = AliceDialogState.AskIntent;
                            AliceDialogDS.Save(dialogSession);
                            return request.Reply("Я могу заброн+ировать стол в заведении The Green Place. Для этого мне надо задать тебе 2 вопроса. Сколько будет гостей. На какое время брон+ируем. Начнем?");
                        }
                        else if (request.Request.OriginalUtterance == "выйти")
                        {
                            dialogSession.State = AliceDialogState.AskIntent;
                            AliceDialogDS.Save(dialogSession);
                            return request.Reply($"{dialogSession.Name} до свидания. Если что, я всегда под рукой!", true);
                        }
                        return request.Reply("Извините пожалуйста, я немного запутался. Что вы хотите? Заброн+ировать стол, получить помощь или выйти?");
                    }
                }
                else if (dialogSession.State == AliceDialogState.Help)
                {
                    if (request.Request.Type == RequestType.SimpleUtterance)
                    {
                        
                    }
                }
                else if (dialogSession.State == AliceDialogState.AskName)
                {
                    if (request.Request.Type == RequestType.SimpleUtterance)
                    {
                        dialogSession.Name = request.Request.OriginalUtterance;
                        dialogSession.State = AliceDialogState.CheckName;
                        AliceDialogDS.Save(dialogSession);
                        return request.Reply($"Я могу называть вас {dialogSession.Name}. Верно?");
                    }
                }
                else if (dialogSession.State == AliceDialogState.CheckName)
                {
                    if (request.Request.Type == RequestType.SimpleUtterance)
                    {
                        if (request.Request.OriginalUtterance.FSpl(" ").Intersect(YesPhrases).Any())
                        {
                            dialogSession.State = AliceDialogState.AskIntent;
                            AliceDialogDS.Save(dialogSession);
                            return request.Reply($"Начнем брон+ировать стол?");
                        }
                        else
                        {
                            dialogSession.State = AliceDialogState.AskName;
                            AliceDialogDS.Save(dialogSession);
                            return request.Reply($"Извините пожалуйста. Скажите ещё раз, как я могу к вам обращаться?");
                        }
                    }
                }
                else if (dialogSession.State == AliceDialogState.AskIntent)
                {
                    if (request.Request.Type == RequestType.SimpleUtterance)
                    {
                        if (request.Request.OriginalUtterance.FSpl(" ").Intersect(YesPhrases).Any())
                        {
                            dialogSession.State = AliceDialogState.AskSeatAmount;
                            AliceDialogDS.Save(dialogSession);
                            return request.Reply("Сколько будет гостей?");
                        }
                        else
                        {
                            dialogSession.State = AliceDialogState.None;
                            AliceDialogDS.Save(dialogSession);
                            return request.Reply("Что вы хотите сделать?");
                        }
                    }
                }
                else if (dialogSession.State == AliceDialogState.AskSeatAmount)
                {
                    if (request.Request.Type == RequestType.SimpleUtterance)
                    {
                        int seats;
                        if (request.Request.OriginalUtterance == "двое")
                        {
                            seats = 2;
                        }
                        else if (request.Request.OriginalUtterance == "трое")
                        {
                            seats = 3;
                        }
                        else if (request.Request.OriginalUtterance == "четверо")
                        {
                            seats = 4;
                        }
                        else if (request.Request.OriginalUtterance == "пятеро")
                        {
                            seats = 5;
                        }
                        else if (request.Request.OriginalUtterance == "шестеро")
                        {
                            seats = 6;
                        }
                        else
                        {
                            try
                            {
                                seats = OnlyNumbers(request).CastVal<int>();
                            }
                            catch (Exception ex)
                            {
                                seats = 0;
                            }
                        }

                        if (seats.Between_LTE_GTE(1, 5))
                        {
                            dialogSession.State = AliceDialogState.AskTime;
                            dialogSession.SeatAmount = seats;
                            AliceDialogDS.Save(dialogSession);
                            return request.Reply("Во сколько вы придете?");
                        }
                        else
                        {
                            dialogSession.State = AliceDialogState.AskSeatAmount;
                            AliceDialogDS.Save(dialogSession);
                            return request.Reply("Я вас не понял. Повторите пожалуйста ещё раз.");
                        }
                    }
                }
                else if (dialogSession.State == AliceDialogState.AskTime)
                {
                    if (request.Request.Type == RequestType.SimpleUtterance)
                    {
                        if (request.Request.OriginalUtterance.FSpl(" ").Intersect(NoPhrases).Any())
                        {
                            dialogSession.State = AliceDialogState.None;
                            AliceDialogDS.Save(dialogSession);
                            return request.Reply("Что вы хотите сделать?");
                        }
                        else
                        {
                            DateTime now = new TimeService().GetNow();
                            DateTime time = now.Date;
                            string total = OnlyNumbers(request);

                            if (total.Length == 1)
                            {
                                time = time.AddHours(total.CastVal<int>());
                                if (time < now)
                                {
                                    time = time.Date.AddHours(12 + total.CastVal<int>());
                                }
                            }
                            else if (total.Length == 2)
                            {
                                time = time.AddHours(total.CastVal<int>());
                            }
                            else if (total.Length == 3 && total.StartsWith('0'))
                            {
                                time = time.AddMinutes(total.Substring(1).CastVal<int>());
                            }
                            else if (total.Length == 3)
                            {
                                time = time.AddHours(total.Remove(1).CastVal<int>());
                                time = time.AddMinutes(total.Substring(1).CastVal<int>());
                                if (time < now)
                                {
                                    time = time.Date.AddHours(12 + total.Remove(1).CastVal<int>());
                                    time = time.AddMinutes(total.Substring(1).CastVal<int>());
                                }
                            }
                            else if (total.Length == 4)
                            {
                                time = time.AddHours(total.Remove(2).CastVal<int>());
                                time = time.AddMinutes(total.Substring(2).CastVal<int>());
                            }
                            else
                            {
                                dialogSession.State = AliceDialogState.AskTime;
                                AliceDialogDS.Save(dialogSession);
                                return request.Reply($"Извините пожалуйста. Скажите ещё раз, на какое время вам нужен стол?");
                            }
                            Console.WriteLine(request.Request.Command);
                            Console.WriteLine(total);
                            Console.WriteLine(time);

                            if (total.Length != 4)
                            {
                                dialogSession.State = AliceDialogState.CheckTime;
                                dialogSession.Time = time;
                                AliceDialogDS.Save(dialogSession);
                                return request.Reply($"Правильно ли я вас понял. В {time:HH:mm}");
                            }
                            else
                            {
                                dialogSession.Time = time;
                                AliceDialogDS.Save(dialogSession);

                                var number = dialogSession.SeatAmount == 1
                                    ? "одного гостя"
                                    : dialogSession.SeatAmount.TrueNumbers("гостя", "-х гостей", "гостей");
                                return request.Reply($"Стол на {number}, заброн+ирован на имя {dialogSession.Name} в {dialogSession.Time:HH:mm}", true);
                            }
                        }
                    }
                }
                else if (dialogSession.State == AliceDialogState.CheckTime)
                {
                    if (request.Request.Type == RequestType.SimpleUtterance)
                    {
                        if (request.Request.OriginalUtterance.FSpl(" ").Intersect(YesPhrases).Any())
                        {
                            var number = dialogSession.SeatAmount == 1
                                    ? "одного гостя"
                                    : dialogSession.SeatAmount.TrueNumbers("гостя", "-х гостей", "гостей");
                            return request.Reply($"Стол на {number}, заброн+ирован на имя {dialogSession.Name} в {dialogSession.Time:HH:mm}", true);
                        }
                        else
                        {
                            dialogSession.State = AliceDialogState.AskTime;
                            AliceDialogDS.Save(dialogSession);
                            return request.Reply($"Извините пожалуйста. Скажите ещё раз, на какое время вам нужен стол?");
                        }
                    }
                }
            }
            return request.Reply("Привет");
        }

        private static string OnlyNumbers(AliceRequest request)
        {
            var total = "";
            foreach (var sym in request.Request.Command)
            {
                if (sym.Between_LTE_GTE('0', '9'))
                {
                    total += sym;
                }
            }

            return total;
        }
    }
}
