﻿using Bronya.Entities;

using Buratino.DI;
using Buratino.Models.DomainService.DomainStructure;

namespace Bronya.Services
{
    /// <summary>
    /// История изменений в БД, инициированных пользователем
    /// </summary>
    public class ProcessTimeLogService
    {
        public IDomainService<ProcessTimeLog> LogDS { get; set; }

        public void LogEvent(Account account, string eventTitle, int milliseconds)
        {
            LogDS = Container.GetDomainService<ProcessTimeLog>(account);
            LogDS.Save(new ProcessTimeLog() { Name = eventTitle, Milliseconds = milliseconds });
        }
    }
}
