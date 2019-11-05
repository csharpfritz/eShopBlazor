using eShopLib;
using eShopLib.ViewModels;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopOnBlazor.Pages
{
  public class IndexBase : ComponentBase
  {
    protected int pageSize = 10;
    protected int pageIndex = 0;
    protected PaginatedItemsViewModel<CatalogItem> Model;

    protected override void OnParametersSet() => LoadPage();

     [Inject]
    public ICatalogService CatalogService { get; set; }

    protected void Previous()
    {
        pageIndex--;
        LoadPage();
    }

    protected void Next()
    {
        pageIndex++;
        LoadPage();
    }

    protected void LoadPage()
    {
        Model = CatalogService.GetCatalogItemsPaginated(pageSize, pageIndex);
    }
  }
}
