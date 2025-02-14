using Bronya.Entities;
using Bronya.Enums;
using Bronya.Xtensions;

using Buratino.API;
using Buratino.Attributes;
using Buratino.DI;
using Buratino.Enums;
using Buratino.Helpers;
using Buratino.Models.Attributes;
using Buratino.Xtensions;

using Telegram.Bot.Types.Enums;

using vkteams.Services;

namespace Bronya.Services
{
    public class BronyaAdministratorService : BronyaServiceBase
    {
        public BronyaAdministratorService(LogService logService, TGAPI tGAPI, Account account) : base(logService, tGAPI, account)
        {
        }

        [ApiPointer("start", "menu")]
        private string Menu()
        {
            return SendOrEdit(
                "Меню администратора:",
                new InlineKeyboardConstructor()
                    .AddButtonDown("График работы", "/work_schedule")
                    .AddButtonDown("🔲 Столы", "/tables")
                    .AddButtonDown("Изменить картинку столов", "/select_table_schema")
                    .AddButtonDown("Изменить текст", "/select_text")
                    .AddButtonDownIf(() => Package.Account.Id == new Guid("4be29f89-f887-48a1-a8af-cad15d032758"), "Роль", "/show_role")
                );
        }

        #region schema
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
        #endregion

        #region workschedule
        [ApiPointer("work_schedule")]
        private string WorkSchedule()
        {
            Package.Account.SelectedSchedule = default;
            AccountService.ResetWaiting(Package.Account);

            TimeService timeService = new TimeService();
            var yesterday = timeService.GetNow().AddDays(-1);
            var oneTimes = BookService.ScheduleService.WorkScheduleDS
                .GetAll(x => x.IsOneTimeSchedule && x.StartDate >= yesterday);

            var constructor = new InlineKeyboardConstructor();
            for (var i = 0; i < 14; i++)
            {
                var date = yesterday.AddDays(i);
                var current = oneTimes.FirstOrDefault(x => x.StartDate == date) ?? BookService.ScheduleService.GetStandartSchedule(date);
                var dayOfWeekTitle = date.DayOfWeek.ToDayOfWeeks().GetAttribute<TitleAttribute>().Description;
                
                if (current == default)
                {
                    constructor.AddButtonDown($"{dayOfWeekTitle} {date:dd.MM} НЕТ РАСПИСАНИЯ", $"/new_schedule");
                    continue;
                }

                var oneTimeTitle = current.IsOneTimeSchedule ? "*" : string.Empty;
                if (current.IsDayOff)
                {
                    constructor.AddButtonDown($"{dayOfWeekTitle} {oneTimeTitle}{date:dd.MM} Не работаем", $"/show_schedule/{current.Id}");
                }
                else
                {
                    constructor.AddButtonDown($"{dayOfWeekTitle} {oneTimeTitle}{date:dd.MM} {current.Start.ToHHmm()}-{current.End.ToHHmm()}", $"/show_schedule/{current.Id}");
                }
            }

            return SendOrEdit(
                $"График работы на 7 дней:",
                constructor
                    .AddButtonDown("Новый график", "/new_schedule")
                    .AddButtonDown("Назад", "/menu")
            );
        }

        [ApiPointer("show_schedule")]
        private string ShowSchedule(WorkSchedule workSchedule)
        {
            return SendOrEdit(
                $"{workSchedule.GetState()}",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Изменить", $"/edit_schedule/{workSchedule.Id}")
                    .AddButtonDown("Назад", "/work_schedule")
            );
        }

        [ApiPointer("new_schedule")]
        private string NewSchedule()
        {
            WorkSchedule entity = new WorkSchedule() { IsDeleted = true };
            var standart = BookService.ScheduleService.GetStandartSchedule();
            if (standart != null)
            {
                entity.Start = standart.Start;
                entity.Length = standart.Length;
                entity.Step = standart.Step;
                entity.Buffer = standart.Buffer;
                entity.MinPeriod = standart.MinPeriod;
                entity.NotificationBeforeBookEnd = standart.NotificationBeforeBookEnd;
                entity.AutoCancelBook = standart.AutoCancelBook;
            }
            var newSchedule = BookService.ScheduleService.WorkScheduleDS.Save(entity);
            return EditSchedule(newSchedule);
        }

        [ApiPointer("edit_schedule")]
        private string EditSchedule(WorkSchedule workSchedule)
        {
            Package.Account.SelectedSchedule = workSchedule;
            AccountService.AccountDS.Save(Package.Account);

            workSchedule.IsDeleted = true;
            BookService.ScheduleService.WorkScheduleDS.Save(workSchedule);

            return SendOrEdit(
                $"{workSchedule.GetState()}",
                new InlineKeyboardConstructor()
                    .AddButtonDown(workSchedule.IsOneTimeSchedule ? "Однодневный график" : "Стандартный график", $"/switch_schedule_onetime")
                    .AddButtonDown(workSchedule.IsOneTimeSchedule ? "Дата" : "Начало действия графика", $"/select_schedule_start_date")
                    .AddButtonDownIf(()=>!workSchedule.IsOneTimeSchedule, "Дни недели", $"/select_schedule_dayofweeks")
                    .AddButtonDown(workSchedule.IsDayOff ? "Не работаем" : "Работаем", $"/switch_schedule_isdayoff")
                    .AddButtonDownIf(() => !workSchedule.IsDayOff, "Начало смены", $"/select_schedule_start")
                    .AddButtonDownIf(() => !workSchedule.IsDayOff && workSchedule.Start != default, "Конец смены", $"/select_schedule_end")
                    .AddButtonDownIf(() => !workSchedule.IsDayOff, "Шаг бронирования", $"/select_schedule_step")
                    .AddButtonDownIf(() => !workSchedule.IsDayOff && workSchedule.Step != default, "Минимальное время брони", $"/select_schedule_minperiod")
                    .AddButtonDownIf(() => !workSchedule.IsDayOff && workSchedule.Step != default, "Буффер", $"/select_schedule_buffer")
                    .AddButtonDownIf(() => !workSchedule.IsDayOff, "Напоминание о заканчивающейся брони", $"/select_schedule_notify")
                    .AddButtonDownIf(() => !workSchedule.IsDayOff, "Авто отмена брони", $"/select_schedule_autocancel")
                    .AddButtonDown("Удалить график", "/work_schedule")
                    .AddButtonRightIf(() => workSchedule.StartDate != default && (workSchedule.IsOneTimeSchedule || !workSchedule.IsOneTimeSchedule && workSchedule.DayOfWeeks != DayOfWeeks.None),
                        "Готово", "/enable_schedule")
            );
        }

        [ApiPointer("enable_schedule")]
        private string SwitchScheduleEnable()
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;
            workSchedule.IsDeleted = false;
            BookService.ScheduleService.WorkScheduleDS.Save(workSchedule);
            return ShowSchedule(workSchedule);
        }

        [ApiPointer("switch_schedule_onetime")]
        private string SwitchScheduleOnetime()
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;
            workSchedule.IsOneTimeSchedule = !workSchedule.IsOneTimeSchedule;
            BookService.ScheduleService.WorkScheduleDS.Save(workSchedule);
            return EditSchedule(workSchedule);
        }

        [ApiPointer("switch_schedule_isdayoff")]
        private string SwitchScheduleIsDayOff()
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;
            workSchedule.IsDayOff = !workSchedule.IsDayOff;
            BookService.ScheduleService.WorkScheduleDS.Save(workSchedule);
            return EditSchedule(workSchedule);
        }

        [ApiPointer("select_schedule_start_date")]
        private string SelectScheduleStartDate()
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;
            AccountService.SetWaiting(Package.Account, WaitingText.ScheduleStartDate);

            return SendOrEdit(
                "Напишите с какого дня начинает действовать график:" +
                "\r\nФормат: д.м.г",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Отмена", $"/edit_schedule/{workSchedule.Id}")
            );
        }

        [ApiPointer("set_schedule_start_date")]
        private string SetScheduleStartDate(string sourceDate)
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;
            var date = ParseDate(sourceDate);
            if (date == default)
            {
                return SendOrEdit(
                    $"Неверный формат. Напишите дату в формате *д.м.г*" +
                    $"\r\nНапишите с какого дня начинает действовать график:",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Отмена", $"/edit_schedule/{workSchedule.Id}")
                );
            }
            AccountService.ResetWaiting(Package.Account);

            workSchedule.StartDate = date;
            BookService.ScheduleService.WorkScheduleDS.Save(workSchedule);

            return EditSchedule(workSchedule);
        }

        [ApiPointer("select_schedule_start")]
        private string SelectScheduleStart()
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;
            Package.Account.Waiting = WaitingText.ScheduleStart;
            AccountService.AccountDS.Save(Package.Account);

            return SendOrEdit(
                "Напишите во сколько начинается смена:" +
                "\r\nФормат: ч:м",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Отмена", $"/edit_schedule/{workSchedule.Id}")
            );
        }

        [ApiPointer("set_schedule_start")]
        private string SetScheduleStart(string sourceTime)
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;
            var time = ParseTime(sourceTime, out string error);
            if (time == default)
            {
                return SendOrEdit(
                    $"{error}" +
                    $"\r\nНапишите во сколько начинается смена:",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Отмена", $"/edit_schedule/{workSchedule.Id}")
                );
            }
            AccountService.ResetWaiting(Package.Account);
            if (workSchedule.End != default && workSchedule.Start != default)
            {
                workSchedule.Length = workSchedule.Length.Add(workSchedule.Start - time);
            }
            workSchedule.Start = time;
            BookService.ScheduleService.WorkScheduleDS.Save(workSchedule);

            return EditSchedule(workSchedule);
        }

        [ApiPointer("select_schedule_end")]
        private string SelectScheduleEnd()
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;
            Package.Account.Waiting = WaitingText.ScheduleEnd;
            AccountService.AccountDS.Save(Package.Account);

            return SendOrEdit(
                "Напишите во сколько заканчивается смена:" +
                "\r\nФормат: ч:м",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Отмена", $"/edit_schedule/{workSchedule.Id}")
            );
        }

        [ApiPointer("set_schedule_end")]
        private string SetScheduleEnd(string sourceTime)
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;
            var time = ParseTime(sourceTime, out string error);
            if (time == default)
            {
                return SendOrEdit(
                    $"{error}" +
                    $"\r\nНапишите во сколько заканчивается смена:",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Отмена", $"/edit_schedule/{workSchedule.Id}")
                );
            }
            AccountService.ResetWaiting(Package.Account);

            if (workSchedule.Start != default && time < workSchedule.Start)
            {
                time = time.Add(new TimeSpan(1, 0, 0, 0));
            }
            workSchedule.Length = time.Subtract(workSchedule.Start);
            BookService.ScheduleService.WorkScheduleDS.Save(workSchedule);

            return EditSchedule(workSchedule);
        }

        [ApiPointer("select_schedule_step")]
        private string SelectScheduleStep()
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;

            return SendOrEdit(
                "Выберите шаг бронирования:",
                new InlineKeyboardConstructor()
                    .AddButtonDown("15 м.", $"/set_schedule_step/{new TimeSpan(0, 15, 0)}")
                    .AddButtonRight("20 м.", $"/set_schedule_step/{new TimeSpan(0, 20, 0)}")
                    .AddButtonDown("30 м.", $"/set_schedule_step/{new TimeSpan(0, 30, 0)}")
                    .AddButtonRight("60 м.", $"/set_schedule_step/{new TimeSpan(0, 60, 0)}")
                    .AddButtonDown("Отмена", $"/edit_schedule/{workSchedule.Id}")
            );
        }

        [ApiPointer("set_schedule_step")]
        private string SetScheduleStep(TimeSpan time)
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;

            workSchedule.Step = time;
            BookService.ScheduleService.WorkScheduleDS.Save(workSchedule);

            return EditSchedule(workSchedule);
        }

        [ApiPointer("select_schedule_minperiod")]
        private string SelectScheduleMinperiod()
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;

            var constructor = new InlineKeyboardConstructor();
            for (int i = 1; i <= 8; i++)
            {
                TimeSpan minperiod = workSchedule.Step.Multiply(i);
                constructor.AddButtonDown($"{minperiod.ToHHmm()}", $"/set_schedule_step/{minperiod}");
            }
            return SendOrEdit(
                "Выберите минимальную продолжительность бронирования:",
                constructor
                    .AddButtonDown("Отмена", $"/edit_schedule/{workSchedule.Id}")
            );
        }

        [ApiPointer("set_schedule_minperiod")]
        private string SetScheduleMinperiod(TimeSpan time)
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;

            workSchedule.MinPeriod = time;
            BookService.ScheduleService.WorkScheduleDS.Save(workSchedule);

            return EditSchedule(workSchedule);
        }

        [ApiPointer("select_schedule_buffer")]
        private string SelectScheduleBuffer()
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;

            var constructor = new InlineKeyboardConstructor();
            for (int i = 1; i <= 3; i++)
            {
                TimeSpan buffer = workSchedule.Step.Multiply(i);
                constructor.AddButtonDown($"{buffer.ToHHmm()}", $"/set_schedule_step/{buffer}");
            }
            return SendOrEdit(
                "Выберите буффер между бронями:",
                constructor
                    .AddButtonDown("Отмена", $"/edit_schedule/{workSchedule.Id}")
            );
        }

        [ApiPointer("set_schedule_buffer")]
        private string SetScheduleBuffer(TimeSpan time)
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;

            workSchedule.Buffer = time;
            BookService.ScheduleService.WorkScheduleDS.Save(workSchedule);

            return EditSchedule(workSchedule);
        }

        [ApiPointer("select_schedule_notify")]
        private string SelectScheduleNotify()
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;

            AccountService.SetWaiting(Package.Account, WaitingText.ScheduleNotify);

            var constructor = new InlineKeyboardConstructor();
            return SendOrEdit(
                "Напишите за сколько *минут* до окончания брони нужно отправлять уведомление хостесу:" +
                "\r\nФормат: минуты",
                constructor
                    .AddButtonDown("Отмена", $"/edit_schedule/{workSchedule.Id}")
            );
        }

        [ApiPointer("set_schedule_notify")]
        private string SetScheduleNotify(int minutes)
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;

            workSchedule.NotificationBeforeBookEnd = new TimeSpan(minutes * 60 * 1000 * 1000 * 100);
            BookService.ScheduleService.WorkScheduleDS.Save(workSchedule);

            AccountService.ResetWaiting(Package.Account);

            return EditSchedule(workSchedule);
        }

        [ApiPointer("select_schedule_autocancel")]
        private string SelectScheduleAutoCancel()
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;

            AccountService.SetWaiting(Package.Account, WaitingText.AutoCancel);

            var constructor = new InlineKeyboardConstructor();
            return SendOrEdit(
                "Напишите через сколько *минут* после начала брони нужно отменять её, если кальян не был вынесен:" +
                "\r\nФормат: минуты",
                constructor
                    .AddButtonDown("Отмена", $"/edit_schedule/{workSchedule.Id}")
            );
        }

        [ApiPointer("set_schedule_autocancel")]
        private string SetScheduleAutoCancel(int minutes)
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;

            workSchedule.AutoCancelBook = new TimeSpan(minutes * 60 * 1000 * 1000 * 100);
            BookService.ScheduleService.WorkScheduleDS.Save(workSchedule);

            AccountService.ResetWaiting(Package.Account);

            return EditSchedule(workSchedule);
        }

        [ApiPointer("select_schedule_dayofweeks")]
        private string SelectScheduleDayOfWeeks()
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;

            var constructor = new InlineKeyboardConstructor();
            foreach (var day in Enum.GetValues<DayOfWeeks>().Except([DayOfWeeks.None, DayOfWeeks.AllDays]))
            {
                var title = day.GetAttribute<TitleAttribute>().Title;
                if (workSchedule.DayOfWeeks.HasFlag(day))
                    title = "✅" + title;
                constructor.AddButtonDown(title, $"/set_schedule_dayofweeks/{day}");
            }

            return SendOrEdit(
                "Выберите дни недели, когда будет действовать этот график:",
                constructor
                    .AddButtonDown("Назад", $"/edit_schedule/{workSchedule.Id}")
            );
        }

        [ApiPointer("set_schedule_dayofweeks")]
        private string SetScheduleDayOfWeeks(DayOfWeeks dayOfWeek)
        {
            WorkSchedule workSchedule = Package.Account.SelectedSchedule;
            if (workSchedule.DayOfWeeks.HasFlag(dayOfWeek))
                workSchedule.DayOfWeeks -= dayOfWeek;
            else
                workSchedule.DayOfWeeks = workSchedule.DayOfWeeks | dayOfWeek;

            BookService.ScheduleService.WorkScheduleDS.Save(workSchedule);
            return SelectScheduleDayOfWeeks();
        }
        #endregion

        #region tables
        [ApiPointer("tables")]
        private string Tables()
        {
            var constructor = new InlineKeyboardConstructor();
            var tables = Container.GetDomainService<Table>(Package.Account).GetAll().OrderBy(x => x.Number).ToArray();
            foreach (var table in tables)
            {
                constructor.AddButtonDown(table.GetTitle(), $"/show_table/{table.Id}");
            }
            constructor.AddButtonDown("Отключить все столы", $"/disable_all_tables");

            return SendOrEdit(
                "Столы:",
                constructor
                    .AddButtonDown("Архив", "/archive")
                    .AddButtonRight("Порядок столов", "/table_order")
                    .AddButtonDown("Назад", "/menu")
                    .AddButtonRight("+Добавить стол", "/new_table"),
                null,
                ImageId
            );
        }

        [ApiPointer("archive")]
        private string Archive()
        {
            var constructor = new InlineKeyboardConstructor();
            var tables = Container.GetRepository<Table>().GetAll(x => x.IsDeleted).OrderBy(x => x.Number).ToArray();
            foreach (var table in tables)
            {
                constructor.AddButtonDown(table.GetTitle(), $"/show_table/{table.Id}");
            }

            return SendOrEdit(
                "Архив столов:",
                constructor
                    .AddButtonDown("Назад", "/tables")
            );
        }

        [ApiPointer("show_table")]
        private string ShowTable(Table table)
        {
            return SendOrEdit(
                $"{table.GetState()}",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Редактировать", $"/edit_table/{table.Id}")
                    .AddButtonDown(table.IsBookAvailable ? "Отключить бронирование" : "Включить бронирование", $"/disable_table/{table.Id}")
                    .AddButtonDown("Назад", "/tables")
            );
        }

        [ApiPointer("disable_table")]
        private string DisableTable(Table table)
        {
            table.IsBookAvailable = !table.IsBookAvailable;
            BookService.TableDS.Save(table);
            return ShowTable(table);
        }

        [ApiPointer("disable_all_tables")]
        private string DisableAllTables()
        {
            var tableDS = Container.GetDomainService<Table>(Package.Account);
            var tables = tableDS.GetAll().ToArray();
            if (tables.All(x => !x.IsBookAvailable))
            {
                tables.Select(x =>
                    {
                        x.IsBookAvailable = true;
                        tableDS.Save(x);
                        return x;
                    })
                    .ToArray();
            }
            else
            {
                tables.Select(x =>
                    {
                        x.IsBookAvailable = false;
                        tableDS.Save(x);
                        return x;
                    })
                    .ToArray();
            }
            return Tables();
        }

        [ApiPointer("edit_table")]
        private string EditTable(Table table)
        {
            AccountService.SelectTable(Package.Account, table);
            return SendOrEdit(
                $"{table.GetState()}",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Название стола", $"/select_table_name")
                    .AddButtonDown("Посадочных мест", $"/select_table_seats")
                    .AddButtonDown(table.HasConsole ? "С приставкой" : "Без приставки", $"/switch_table_console")
                    .AddButtonDown(table.IsDeleted ? "Восстановить из архива" : "Убрать в архив", $"/switch_table_delete")
                    .AddButtonDown("Назад", "/tables")
            );
        }

        [ApiPointer("switch_table_console")]
        private string SwitchTableConsole()
        {
            var table = Package.Account.SelectedTable;
            table.HasConsole = !table.HasConsole;
            BookService.TableDS.Save(table);
            return EditTable(table);
        }

        [ApiPointer("switch_table_delete")]
        private string SwitchTableDelete()
        {
            var table = Package.Account.SelectedTable;
            table.IsDeleted = !table.IsDeleted;
            BookService.TableDS.Save(table);
            return EditTable(table);
        }

        [ApiPointer("select_table_seats")]
        private string SelectTableSeats()
        {
            var constructor = new InlineKeyboardConstructor();
            int count = 0;
            int tablesInRow = 3;
            for (int i = 1; i <= 12; i++)
            {
                if (count == tablesInRow)
                {
                    count = 0;
                    constructor.AddButtonDown(i.ToString(), $"/set_table_seats/{i}");
                }
                else
                {
                    constructor.AddButtonRight(i.ToString(), $"/set_table_seats/{i}");
                }
                count++;
            }

            return SendOrEdit(
                "Выберите сколько помещяется человек за столом:",
                constructor
                    .AddButtonDown("Отмена", $"/edit_table/{Package.Account.SelectedTable.Id}")
            );
        }

        [ApiPointer("set_table_seats")]
        private string SelectTableSeats(int seats)
        {
            Package.Account.SelectedTable.NormalSeatAmount = seats;
            BookService.TableDS.Save(Package.Account.SelectedTable);
            return EditTable(Package.Account.SelectedTable);
        }

        [ApiPointer("select_table_name")]
        private string SelectTableName()
        {
            AccountService.SetWaiting(Package.Account, WaitingText.TableName);
            return SendOrEdit(
                "Напишите название стола:",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Отмена", $"/edit_table/{Package.Account.SelectedTable.Id}")
            );
        }

        [ApiPointer("set_table_name")]
        private string SelectTableName(string name)
        {
            AccountService.ResetWaiting(Package.Account);
            Package.Account.SelectedTable.Name = name;
            BookService.TableDS.Save(Package.Account.SelectedTable);
            return EditTable(Package.Account.SelectedTable);
        }

        [ApiPointer("table_order")]
        private string TableOrder()
        {
            var tables = BookService.TableDS.GetAll().OrderBy(x => x.Number).ToList();
            var constructor = new InlineKeyboardConstructor();
            foreach (var table in tables)
            {
                if (table == tables.First())
                {
                    constructor.AddButtonDown($"{table.Name}🔻", $"/table_order_down/{table.Id}");
                }
                else if (table == tables.Last())
                {
                    constructor.AddButtonDown($"{table.Name}🔺", $"/table_order_up/{table.Id}");
                }
                else
                {
                    constructor.AddButtonDown($"{table.Name}🔺", $"/table_order_up/{table.Id}");
                    constructor.AddButtonRight($"{table.Name}🔻", $"/table_order_down/{table.Id}");
                }
            }
            return SendOrEdit("Порядок столов:",
                constructor
                    .AddButtonDown("Готово", $"/tables")
                );
        }

        [ApiPointer("table_order_down")]
        private string TableOrderDown(Table table)
        {
            var tables = BookService.TableDS.GetAll().OrderBy(x => x.Number).ToList();
            var constructor = new InlineKeyboardConstructor();
            for (var i = 0; i < tables.Count; i++)
            {
                tables[i].Number = i;
                if (tables[i] == table)
                {
                    if (i < tables.Count - 1)
                    {
                        tables[i].Number = i + 1;
                        tables[i + 1].Number = i;
                        i++;
                        continue;
                    }
                }
            }
            tables.Select(x =>
            {
                BookService.TableDS.Save(x);
                return x;
            }).ToArray();
            return TableOrder();
        }

        [ApiPointer("table_order_up")]
        private string TableOrderUp(Table table)
        {
            var tables = BookService.TableDS.GetAll().OrderBy(x => x.Number).ToList();
            var constructor = new InlineKeyboardConstructor();
            for (var i = 0; i < tables.Count; i++)
            {
                tables[i].Number = i;
                if (tables[i] == table)
                {
                    if (i > 0)
                    {
                        tables[i].Number = i - 1;
                        tables[i - 1].Number = i;
                        i++;
                        continue;
                    }
                }
            }
            tables.Select(x =>
            {
                BookService.TableDS.Save(x);
                return x;
            }).ToArray();
            return TableOrder();
        }

        [ApiPointer("new_table")]
        private string NewTable()
        {
            var table = BookService.TableDS.Save(new Table());

            return EditTable(table);
        }
        #endregion

        private DateTime ParseDate(string date)
        {
            var parts = date.FSpl(".").Select(x => x.AsInt()).ToArray();
            if (parts.Length == 3)
            {
                if (parts[2] < 100)
                    parts[2] += 2000;
                return new DateTime(parts[2], parts[1], parts[0]);
            }
            else if (parts.Length == 2)
            {
                return new DateTime(new TimeService().GetNow().Year, parts[1], parts[0]);
            }
            else
            {
                return default;
            }
        }

        private TimeSpan ParseTime(string time, out string error)
        {
            error = string.Empty;
            var parts = time.FSpl(":").Select(x => x.AsInt()).ToArray();
            if (parts.Length == 2)
            {
                return new TimeSpan(parts[0], parts[1], 0);
            }
            else if (parts.Length == 1)
            {
                return new TimeSpan(parts[0], 0, 0);
            }
            else
            {
                error = "Неверный формат. Напишите время в формате *ч:м*";
                return default;
            }
        }
    }
}