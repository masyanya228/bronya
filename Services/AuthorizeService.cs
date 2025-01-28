using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Buratino.Models.DomainService.DomainStructure;
using Buratino.Entities;
using Buratino.DI;
using vkteams.Services;
using Buratino.API;

namespace Bronya.Services
{
    public class AuthorizeService
    {
        IBronyaServiceBase BronyaService;
        IBronyaServiceBase BronyaHostesService;
        protected AccountService AccountService;
        protected IDomainService<RoleAccountLink> RoleDS;

        public LogService LogService { get; }
        public TGAPI TgAPI { get; }

        public AuthorizeService(LogService logService, TGAPI tgAPI)
        {
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
            if (roles.Any(x=>x.Role.Name=="Administrator"))
            {
                throw new NotImplementedException();
            }
            else if (roles.Any(x => x.Role.Name == "Hostes"))
            {
                return BronyaHostesService.OnUpdateWrapper(dataPackage);
            }
            else
            {
                return BronyaService.OnUpdateWrapper(dataPackage);
            }
        }
    }
}
