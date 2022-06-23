using eShopLib;
using eShopLib.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace eShopOnMaui.Services;

public class CatalogService : ICatalogService
{

	private readonly string _BaseURL;

	public CatalogService()
	{
		_BaseURL = DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:51190" : "https://localhost:51189/";
	}

	private HttpClient GetHttpClient() 
	{

		return new HttpClient
		{
			BaseAddress = new Uri(_BaseURL)
		};

	}

	public PaginatedItemsViewModel<CatalogItem> GetCatalogItemsPaginated(int pageSize, int pageIndex)
	{

		var items = GetHttpClient().GetFromJsonAsync<PaginatedItemsViewModel<CatalogItem>>("/api/Catalog", 
			new System.Text.Json.JsonSerializerOptions {
				PropertyNameCaseInsensitive = true
			}).GetAwaiter().GetResult();

		var totalItems = items.TotalItems;

		var itemsOnPage = items.Data
				.OrderBy(c => c.Id)
				.Skip(pageSize * pageIndex)
				.Take(pageSize)
				.ToList();

		return new PaginatedItemsViewModel<CatalogItem>(
				pageIndex, pageSize, totalItems, itemsOnPage);
	}

	public CatalogItem FindCatalogItem(int id)
	{
		var item = GetHttpClient().GetFromJsonAsync<CatalogItem>($"/api/Catalog/{id}").GetAwaiter().GetResult();
		return item;

	}
	public IEnumerable<CatalogType> GetCatalogTypes()
	{
		return Enumerable.Empty<CatalogType>();
	}

	public IEnumerable<CatalogBrand> GetCatalogBrands()
	{
		return Enumerable.Empty<CatalogBrand>();
	}

	public void CreateCatalogItem(CatalogItem catalogItem)
	{
		// do nothing
	}

	public void UpdateCatalogItem(CatalogItem catalogItem)
	{
		// do nothing
	}

	public void RemoveCatalogItem(CatalogItem catalogItem)
	{
		// do nothing
	}

	public void Dispose()
	{
		// do nothing
	}
}