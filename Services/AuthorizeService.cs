using Telegram.Bot.Types;
using Buratino.Models.DomainService.DomainStructure;
using Buratino.Entities;
using Buratino.DI;
using vkteams.Services;
using Buratino.API;
using Bronya.Dtos;

namespace Bronya.Services
{
    public class AuthorizeService
    {
        public static AuthorizeService Instance { get; private set; }
        IBronyaServiceBase BronyaService;
        IBronyaServiceBase BronyaHostesService;
        protected AccountService AccountService;
        protected IDomainService<RoleAccountLink> RoleDS;

        public LogService LogService { get; }
        public TGAPI TgAPI { get; }

        public AuthorizeService(LogService logService, TGAPI tgAPI)
        {
            Instance = this;
            TgAPI = tgAPI;
            LogService = logService;

            AccountService = new AccountService();
            RoleDS = Container.GetDomainService<RoleAccountLink>();
            BronyaService = new BronyaService(logService, TgAPI);
            BronyaHostesService = new BronyaHostesService(logService, TgAPI);
            
            TgAPI.UpdateEvent += OnUpdateWrapper;
            tgAPI.Start();
        }

        private Task OnUpdateWrapper(object sender, Update update)
        {
            var acc = AccountService.GetAccount(update);
            var dataPackage = new DataPackage(acc, update);
            var roles = RoleDS.GetAll().Where(x => x.Account.Id == acc.Id).ToList();
            if (IsAdministrator(acc))
            {
                throw new NotImplementedException();
            }
            else if (IsHostes(acc))
            {
                return BronyaHostesService.OnUpdateWrapper(dataPackage);
            }
            else
            {
                return BronyaService.OnUpdateWrapper(dataPackage);
            }
        }

        public bool IsHostes(Account account)
        {
            var roles = RoleDS.GetAll().Where(x => x.Account.Id == account.Id).ToList();
            return roles.Any(x => x.Role.Name == "Hostes");
        }
        public bool IsAdministrator(Account account)
        {
            var roles = RoleDS.GetAll().Where(x => x.Account.Id == account.Id).ToList();
            return roles.Any(x => x.Role.Name == "Administrator");
        }
    }
}
