using Buratino.Attributes;

namespace Buratino.Enums
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
    }
}
