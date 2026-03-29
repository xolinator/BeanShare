using System.Net.Http.Json;
using BeanShare.Application.Features.ActiveQrCodes.Dtos;

namespace BeanShare.Maui.Services;

public class ActiveQrService : IActiveQrService
{
    private readonly HttpClient _httpClient;

    public ActiveQrService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ActiveQrCodeDto?> CreateActiveQrCodeAsync(Guid spaceId, string label, string productName, string productBrand, string productType, string recipeName, int defaultGrams)
    {
        try
        {
            var request = new
            {
                Label = label,
                ProductName = productName,
                ProductBrand = productBrand,
                ProductType = productType,
                RecipeName = recipeName,
                DefaultGrams = defaultGrams
            };

            var response = await _httpClient.PostAsJsonAsync($"/api/spaces/{spaceId}/qr-codes", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ActiveQrCodeDto>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<ActiveQrCodeDto>?> GetActiveQrCodesAsync(Guid spaceId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<ActiveQrCodeDto>>($"/api/spaces/{spaceId}/qr-codes");
        }
        catch
        {
            return null;
        }
    }

    public async Task<ActiveQrCodeDto?> ResolveActiveQrCodeAsync(Guid qrCodeId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ActiveQrCodeDto>($"/api/qr-codes/{qrCodeId}/resolve");
        }
        catch
        {
            return null;
        }
    }

    public async Task<ActiveQrCodeDto?> ReassignActiveQrCodeAsync(Guid qrCodeId, string productName, string productBrand, string productType, string recipeName, int defaultGrams)
    {
        try
        {
            var request = new
            {
                ProductName = productName,
                ProductBrand = productBrand,
                ProductType = productType,
                RecipeName = recipeName,
                DefaultGrams = defaultGrams
            };

            var response = await _httpClient.PutAsJsonAsync($"/api/qr-codes/{qrCodeId}/reassign", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ActiveQrCodeDto>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeactivateActiveQrCodeAsync(Guid qrCodeId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/qr-codes/{qrCodeId}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
