﻿using Bronya.Attributes;

namespace Bronya.Enums
{
    public enum WaitingText
    {
        None = 0,

        /// <summary>
        /// Хостес указывает имя брони
        /// </summary>
        [ApiPointer("set_name")]
        Name,

        /// <summary>
        /// Хостес редактирует номер гостя
        /// </summary>
        [ApiPointer("set_phone")]
        PhoneNumber,

        /// <summary>
        /// Хостес редактирует карту гостя
        /// </summary>
        [ApiPointer("set_card")]
        CardNumber,

        /// <summary>
        /// Хостес редактирует имя гостя
        /// </summary>
        [ApiPointer("set_acc_name")]
        AccName,

        /// <summary>
        /// Администратор обновляет схему расположения столов
        /// </summary>
        [ApiPointer("set_table_schema")]
        TableSchemaImage,

        /// <summary>
        /// Администратор устанавливает дату начала графика
        /// </summary>
        [ApiPointer("set_schedule_start_date")]
        ScheduleStartDate,

        /// <summary>
        /// Администратор устанавливает начало смены
        /// </summary>
        [ApiPointer("set_schedule_start")]
        ScheduleStart,

        /// <summary>
        /// Администратор устанавливает конец смены
        /// </summary>
        [ApiPointer("set_schedule_end")]
        ScheduleEnd,

        /// <summary>
        /// Администратор устанавливает время уведомления до закрытия стола
        /// </summary>
        [ApiPointer("set_schedule_notify")]
        ScheduleNotify,

        /// <summary>
        /// Администратор устанавливает время, когда бронь будет автоматически отменена
        /// </summary>
        [ApiPointer("set_schedule_autocancel")]
        AutoCancel,

        /// <summary>
        /// Администратор устанавливает название стола
        /// </summary>
        [ApiPointer("set_table_name")]
        TableName,

        /// <summary>
        /// У пользователя запрашивается телефон. Этот атрибут для метода, который указывает на ошибочное заполнение телефона
        /// </summary>
        [ApiPointer("phone_by_text")]
        AskPhone,

        /// <summary>
        /// Администратор устанавливает статичный текст
        /// </summary>
        [ApiPointer("set_static_text")]
        SetStatickText,

        /// <summary>
        /// Администратор устанавливает правила для гостей заведения
        /// </summary>
        [ApiPointer("set_rules")]
        SetRules,

        /// <summary>
        /// Администратор устанавливает меню в PDF
        /// </summary>
        [ApiPointer("set_menu")]
        SetMenu,
    }
}
