using Telegram.Bot.Types;
using Buratino.API;
using Bronya.Dtos;
using Bronya.Entities;
using Bronya.Enums;

namespace Bronya.Services
{
    public class AuthorizeService
    {
        public static AuthorizeService Instance { get; private set; }
        public AccountService AccountService;

        public LogToFileService LogService { get; }
        public TGAPI TgAPI { get; }

        public AuthorizeService(LogToFileService logService, TGAPI tgAPI)
        {
            Instance = this;
            TgAPI = tgAPI;
            LogService = logService;

            AccountService = new AccountService(null);
            
            TgAPI.UpdateEvent += OnUpdateWrapper;
            tgAPI.Start();
        }

        private Task OnUpdateWrapper(object sender, Update update)
        {
            var acc = AccountService.GetAccount(update);
            var dataPackage = new DataPackage(acc, update);
            var role = GetRole(acc);
            return role switch
            {
                RoleType.Administrator => new BronyaAdministratorService(LogService, TgAPI, acc).OnUpdateWrapper(dataPackage),
                RoleType.Hostes => new BronyaHostesService(LogService, TgAPI, acc).OnUpdateWrapper(dataPackage),
                _ => new BronyaService(LogService, TgAPI, acc).OnUpdateWrapper(dataPackage)
            };
        }

        public RoleType GetRole(Account account)
        {
            if (account.Roles.Any(x => x.Name == "Administrator"))
                return RoleType.Administrator;
            else if (account.Roles.Any(x => x.Name == "Hostes"))
                return RoleType.Hostes;
            else
                return RoleType.Costumer;
        }
    }
}
