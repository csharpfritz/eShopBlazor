using BlazorWebFormsComponents;
using eShopLib;
using eShopLib.ViewModels;
using eShopOnBlazor.Components;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopOnBlazor.Pages
{
	public partial class Index
	{

		protected ListView<CatalogItem> productList;

		int pageSize = 10;
		int pageIndex = 0;
		PaginatedItemsViewModel<CatalogItem> Model;

		[Inject]
		public ICatalogService CatalogService { get; set; }

		protected override void OnParametersSet() => LoadPage();

		void Previous()
		{
			pageIndex--;
			LoadPage();
		}

		void Next()
		{
			pageIndex++;
			LoadPage();
		}

		void LoadPage()
		{
			Model = CatalogService.GetCatalogItemsPaginated(pageSize, pageIndex);
		}

		protected override void OnAfterRender(bool firstRender)
		{

			productList.DataSource = Model.Data;
			productList.DataBind();

		}

		private static class Container
		{

			public static object DataItem { get; set; }

		}


	}
}
