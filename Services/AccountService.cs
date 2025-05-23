﻿using Bronya.Caching.Structure;
using Bronya.DI;
using Bronya.DomainServices.DomainStructure;
using Bronya.Entities;
using Bronya.Enums;

using Buratino.Xtensions;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bronya.Services
{
    public class AccountService
    {
        public static readonly Account RootAccount = new() { Id = new Guid("da8c13be-6d97-4287-b47e-34caada8d315") };
        public static readonly Account MainTester = new() { Id = new Guid("4be29f89-f887-48a1-a8af-cad15d032758"), TGChatId = "1029089379" };
        public static readonly Account AliceRoot = new() { Id = new Guid("feb095a9-25be-42ae-b6a7-a48243636f37") };
        public IDomainService<Account> AccountDS { get; set; }
        public ICacheService<Account> AccountCacheService { get; set; }

        public AccountService(Account account)
        {
            AccountDS = Container.GetDomainService<Account>(account);
            AccountCacheService = Container.Get<ICacheService<Account>>();
        }

        public Account GetAccount(Update update)
        {
            var chatId = update.Message?.Chat?.Id ?? update.CallbackQuery?.Message?.Chat?.Id ?? throw new NotImplementedException("Такой тип события не поддерживается");
            Account acc = GetAccountWithCache(chatId.ToString());
            if (acc is null)
            {
                string name = string.Empty;
                string lastName = string.Empty;
                string tgTag = string.Empty;
                if (update.Type == UpdateType.Message)
                {
                    name = update.Message.From.FirstName;
                    lastName = update.Message.From.LastName;
                    tgTag = update.Message.From.Username;
                }
                else if (update.Type == UpdateType.CallbackQuery)
                {
                    name = update.CallbackQuery.From.FirstName;
                    lastName = update.CallbackQuery.From.LastName;
                    tgTag = update.CallbackQuery.From.Username;
                }
                acc = new Account()
                {
                    Name = string.Join(" ", [name, lastName]),
                    TGChatId = chatId.ToString(),
                    TGTag = tgTag
                };
                AccountDS.Save(acc);
            }
            return acc;
        }

        public Account GetAccountWithCache(string chatId)
        {
            return AccountCacheService.Get(() => GetAccountByChatId(chatId), chatId);
        }

        public Account GetAccountByChatId(string chatId)
        {
            return AccountDS.GetAll().Where(x => x.TGChatId == chatId).SingleOrDefault();
        }

        public IEnumerable<Account> FindAccount(string fullName, bool allowOnlyName)
        {
            var args = fullName.TrueSplit(",");
            string number = default;
            if (!allowOnlyName && args[1] == default)
            {
                return null;
            }

            if (args[1] != default)
            {
                number = ParseNumber(args[1]);
                var byPhone = AccountDS.GetAll().Where(x => x.Phone == number).ToList();
                var byCardNumber = AccountDS.GetAll().Where(x => x.CardNumber == number).ToList();
                if (byPhone.Count != 0)
                {
                    if (byPhone.Count == 1)
                    {
                        return [byPhone.Single()];
                    }
                    else
                    {
                        var byName = byPhone.SingleOrDefault(x => x.Name.ToLower() == args[0].ToLower());
                        if (byName != default)
                            return [byName];
                        else
                            return byPhone;
                    }
                }
                else if (byCardNumber.Count != 0)
                {
                    if (byCardNumber.Count == 1)
                    {
                        return [byCardNumber.Single()];
                    }
                    else
                    {
                        var byName = byCardNumber.SingleOrDefault(x => x.Name.ToLower() == args[0].ToLower());
                        if (byName != default)
                            return [byName];
                        else
                            return byCardNumber;
                    }
                }
            }
            var newAcc = new Account() { Name = args[0] };
            if (number != default)
            {
                if (number.Length == 4)
                {
                    newAcc.CardNumber = number;
                }
                else
                {
                    newAcc.Phone = number;
                }
            }
            AccountDS.Save(newAcc);
            return [newAcc];
        }

        public string ParseNumber(string source)
        {
            var phone = source.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            if (phone.Length == 4)
                return phone;
            if (phone.Length == 10)
                return "+7" + phone;
            else if (phone.Length == 11 && phone.StartsWith('8'))
                return string.Concat("+7", phone[1..]);
            else if (phone.Length == 11 && phone.StartsWith('7'))
                return "+" + phone;
            else
                return phone;
        }

        public IEnumerable<Account> GetAccountsToUnion(Account account)
        {
            return AccountDS.GetAll()
                .Where(x => account.CardNumber != default && account.CardNumber == x.CardNumber
                    || account.Phone != default && account.Phone == x.Phone)
                //.Where(x => x.GetCardNumber != default || x.GetPhone != default)
                //.Where(x => x.GetCardNumber == account.GetCardNumber || x.GetPhone == account.GetPhone)
                .Where(x => x.Id != account.Id && x.IsFake())
                .ToArray();
        }

        public Account GetTrueAccount(Account account)
        {
            return account.IsFake()
                ? AccountDS.GetAll()
                    .Where(x => account.CardNumber != default && account.CardNumber == x.CardNumber
                        || account.Phone != default && account.Phone == x.Phone)
                    //.Where(x => x.GetCardNumber != default || x.GetPhone != default)
                    //.Where(x => x.GetCardNumber == account.GetCardNumber || x.GetPhone == account.GetPhone)
                    .Where(x => x.Id != account.Id && !x.IsFake())
                    .FirstOrDefault()
                : default;
        }

        public bool ResetWaiting(Account account)
        {
            account.Waiting = WaitingText.None;
            AccountDS.Save(account);
            return true;
        }

        public bool SetWaiting(Account account, WaitingText waitingText)
        {
            account.Waiting = waitingText;
            AccountDS.Save(account);
            return true;
        }

        public bool SelectTable(Account account, Table table)
        {
            account.SelectedTable = table;
            AccountDS.Save(account);
            return true;
        }

        public bool SelectBook(Account account, Book book)
        {
            account.SelectedBook = book;
            AccountDS.Save(account);
            return true;
        }
    }
}
