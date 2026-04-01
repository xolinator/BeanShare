using BeanShare.Application.Features.CoffeeStock.Dtos;

namespace BeanShare.Maui.Services;

public interface IStockService
{
    Task<CoffeeStockDto?> GetSpaceStockAsync(Guid spaceId);
    Task<bool> AddStockPurchaseAsync(Guid spaceId, string productName, string coffeeType, int quantityGrams, decimal totalCost, string currency, DateTime purchasedAt);
}
