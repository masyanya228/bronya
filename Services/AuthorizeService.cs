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
        public AccountService AccountService { get; set; }
        public LogToFileService LogService { get; }
        public TGAPI TgAPI { get; }
        public ExceptionLogService ExceptionLogService { get; set; }
        public ProcessTimeLogService processTimeLogService { get; set; }

        public AuthorizeService(LogToFileService logService, TGAPI tgAPI)
        {
            Instance = this;
            TgAPI = tgAPI;
            LogService = logService;

            AccountService = new(null);
            ExceptionLogService = new();
            processTimeLogService = new();

            TgAPI.UpdateEvent += OnUpdateWrapper;
            tgAPI.Start();
        }

        private Task OnUpdateWrapper(object sender, Update update)
        {
            Account acc = null;
            try
            {
                var start = Environment.TickCount;
                acc = AccountService.GetAccount(update);
                var dataPackage = new DataPackage(acc, update);
                var role = GetRole(acc);
                var res = role switch
                {
                    RoleType.Administrator => new BronyaAdministratorService(LogService, TgAPI, acc).OnUpdateWrapper(dataPackage),
                    RoleType.Hostes => new BronyaHostesService(LogService, TgAPI, acc).OnUpdateWrapper(dataPackage),
                    _ => new BronyaService(LogService, TgAPI, acc).OnUpdateWrapper(dataPackage)
                };

                var diff = Environment.TickCount - start;
                if (diff > 100)
                {
                    processTimeLogService.LogEvent(acc, dataPackage.Command, diff);
                }
                return res;
            }
            catch (Exception e)
            {
                ExceptionLogService.LogEvent(acc, e);
                throw;
            }
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
