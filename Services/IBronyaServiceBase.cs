using Buratino.Entities;

using Telegram.Bot.Types;

namespace vkteams.Services
{
    public interface IBronyaServiceBase
    {
        Task OnUpdateWrapper(Update update, Account acc);
    }
}