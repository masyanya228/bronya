using Buratino.API;
using Buratino.Attributes;
using Buratino.Xtensions;
using Buratino.Helpers;
using Buratino.Models.DomainService.DomainStructure;
using Bronya.Entities;
using Buratino.DI;
using Buratino.Entities;
using Bronya.Services;

namespace vkteams.Services
{
    /// <summary>
    /// Сервис взаимодействия с пользователем через telegram
    /// </summary>
    public class BronyaHostesService : BronyaServiceBase
    {
        public IDomainService<Table> TableDS { get; set; } = Container.GetDomainService<Table>();
        public IDomainService<Book> BookDS { get; set; } = Container.GetDomainService<Book>();
        public BronyaHostesService(LogService logService, TGAPI tGAPI) : base(logService, tGAPI)
        {
        }

        [TGPointer("start", "menu")]
        private string Menu(long chatId, int messageId = 0)
        {
            return TGAPI.SendOrEdit(chatId,
                "Меню хостеса:",
                messageId,
                new InlineKeyboardConstructor()
                    .AddButtonDown("Столы", "/tables"));
        }

        [TGPointer("tables")]
        private string Tables(long chatId, int messageId = 0)
        {
            return TGAPI.SendOrEdit(chatId,
                "Меню хостеса:",
                messageId,
                new InlineKeyboardConstructor()
                    .AddHostesTableButtons());
        }
    }
}
