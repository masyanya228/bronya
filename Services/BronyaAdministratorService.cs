using Bronya.Entities;

using Buratino.API;
using Buratino.Attributes;
using Buratino.Enums;
using Buratino.Helpers;
using Buratino.Xtensions;

using Telegram.Bot.Types.Enums;

using vkteams.Services;

namespace Bronya.Services
{
    public class BronyaAdministratorService : BronyaServiceBase
    {
        public BronyaAdministratorService(LogService logService, TGAPI tGAPI) : base(logService, tGAPI)
        {
        }

        [ApiPointer("start", "menu")]
        private string Menu()
        {
            return SendOrEdit(
                "Меню администратора:",
                new InlineKeyboardConstructor()
                    .AddButtonDown("График работы", "/now")
                    .AddButtonDown("🔲 Столы", "/tables")
                    .AddButtonDown("Изменить текст", "/select_text")
                    .AddButtonDown("Изменить картинку столов", "/select_table_schema")
                    .AddButtonDown("Изменить ", "/get_accounts")
                    .AddButtonDownIf(() => Package.Account.Id == new Guid("4be29f89-f887-48a1-a8af-cad15d032758"), "Роль", "/show_role")
                );
        }

        [ApiPointer("cancel_select_table_schema")]
        private string CancelSelectTableSchema()
        {
            Package.Account.Waiting = WaitingText.None;
            AccountService.AccountDS.Save(Package.Account);
            return Menu();
        }

        [ApiPointer("select_table_schema")]
        private string SelectTableSchema()
        {
            Package.Account.Waiting = WaitingText.TableSchemaImage;
            AccountService.AccountDS.Save(Package.Account);

            return SendOrEdit(
                "Пришлите картнку со схемой столов",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Отмена", "/cancel_select_table_schema")
            );
        }

        [ApiPointer("set_table_schema")]
        private string SetTableSchema()
        {
            if (Package.Update.Message.Type == MessageType.Photo)
            {
                var fileId = Package.Update.Message.Photo.Last().FileId;
                if (string.IsNullOrEmpty(fileId))
                    throw new Exception("Не получилось обработать картинку");
                TableSchemaImageDS.Save(new TableSchemaImage() { ImageId = fileId });
                ImageId = fileId;
            }

            Package.Account.Waiting = WaitingText.None;
            AccountService.AccountDS.Save(Package.Account);

            return SendOrEdit(
                "Схема столов обновлена",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Отмена", "/menu"),
                null,
                ImageId
            );
        }
    }
}