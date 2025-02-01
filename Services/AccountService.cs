﻿using Buratino.DI;
using Buratino.Entities;
using Buratino.Models.DomainService.DomainStructure;
using Buratino.Xtensions;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bronya.Services
{
    public class AccountService
    {
        public IDomainService<Account> AccountDS { get; set; } = Container.GetDomainService<Account>();

        public Account GetAccount(Update update)
        {
            var chatId = update.Message?.Chat?.Id ?? update.CallbackQuery?.Message?.Chat?.Id ?? throw new NotImplementedException("Такой тип события не поддерживается");
            var acc = AccountDS.GetAll().Where(x => x.TGChatId == chatId.ToString()).SingleOrDefault();
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

        public IEnumerable<Account> FindAccount(string fullName, bool allowOnlyName)
        {
            var args = fullName.TrueSplit(",");
            if (!allowOnlyName && args[1] == default)
            {
                return null;
            }

            if (args[1] != default)
            {
                var byPhone = AccountDS.GetAll().Where(x => x.Phone == args[1]).ToList();
                var byCardNumber = AccountDS.GetAll().Where(x => x.CardNumber == args[1]).ToList();
                if (byPhone.Any())
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
                else if (byCardNumber.Any())
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
            AccountDS.Save(newAcc);
            return [newAcc];
        }
    }
}
