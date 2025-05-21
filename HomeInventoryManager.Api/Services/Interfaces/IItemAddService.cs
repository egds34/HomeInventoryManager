using HomeInventoryManager.Api.Utilities;
using HomeInventoryManager.Dto;

namespace HomeInventoryManager.Api.Services.Interfaces
{
    public interface IItemAddService
    {
        Task<ServiceResult<ItemAddDto>> AddItemAsync(ItemAddDto item);
    }
}
