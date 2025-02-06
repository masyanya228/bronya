using Bronya.Entities;

using Buratino.DI;
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
            else if (phone.Length == 11 && phone.StartsWith("8"))
                return "+7" + phone.Substring(1);
            else if (phone.Length == 11 && phone.StartsWith("7"))
                return "+" + phone;
            else
                return phone;
        }

        public IEnumerable<Account> GetAccountsToUnion(Account account)
        {
            return AccountDS.GetAll()
                .Where(x=> account.CardNumber != default && account.CardNumber == x.CardNumber 
                    || account.Phone != default && account.Phone == x.Phone)
                //.Where(x => x.CardNumber != default || x.Phone != default)
                //.Where(x => x.CardNumber == account.CardNumber || x.Phone == account.Phone)
                .Where(x => x.Id != account.Id && x.TGTag == default)
                .ToArray();
        }

        public Account GetTrueAccount(Account account)
        {
            return account.TGTag == default
                ? AccountDS.GetAll()
                    .Where(x => account.CardNumber != default && account.CardNumber == x.CardNumber
                        || account.Phone != default && account.Phone == x.Phone)
                    //.Where(x => x.CardNumber != default || x.Phone != default)
                    //.Where(x => x.CardNumber == account.CardNumber || x.Phone == account.Phone)
                    .Where(x => x.Id != account.Id && x.TGTag != default)
                    .FirstOrDefault()
                : default;
        }
    }
}
