using Bronya.Dtos;

namespace vkteams.Services
{
    public interface IBronyaServiceBase
    {
        Task OnUpdateWrapper(DataPackage dataPackage);
    }
}