using Telegram.Bot.Types;
using Buratino.Models.DomainService.DomainStructure;
using Buratino.DI;
using vkteams.Services;
using Buratino.API;
using Bronya.Dtos;
using Buratino.Enums;
using Bronya.Entities;

namespace Bronya.Services
{
    public class AuthorizeService
    {
        public static AuthorizeService Instance { get; private set; }
        IBronyaServiceBase BronyaService;
        IBronyaServiceBase BronyaHostesService;
        IBronyaServiceBase BronyaAdministratorService;
        public AccountService AccountService;

        public LogService LogService { get; }
        public TGAPI TgAPI { get; }

        public AuthorizeService(LogService logService, TGAPI tgAPI)
        {
            Instance = this;
            TgAPI = tgAPI;
            LogService = logService;

            AccountService = new AccountService();
            BronyaService = new BronyaService(logService, TgAPI);
            BronyaHostesService = new BronyaHostesService(logService, TgAPI);
            BronyaAdministratorService = new BronyaAdministratorService(logService, TgAPI);
            
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
                RoleType.Administrator => BronyaAdministratorService.OnUpdateWrapper(dataPackage),
                RoleType.Hostes => BronyaHostesService.OnUpdateWrapper(dataPackage),
                _ => BronyaService.OnUpdateWrapper(dataPackage)
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
