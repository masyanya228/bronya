using Bronya.Dtos;

namespace Bronya.Services
{
    public interface IBronyaServiceBase
    {
        BookService BookService { get; set; }
        Task OnUpdateWrapper(DataPackage dataPackage);
    }
}