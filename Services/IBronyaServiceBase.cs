using Bronya.Dtos;
using Bronya.Services;

namespace vkteams.Services
{
    public interface IBronyaServiceBase
    {
        BookService BookService { get; set; }
        Task OnUpdateWrapper(DataPackage dataPackage);
    }
}