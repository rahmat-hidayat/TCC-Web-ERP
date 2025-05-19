using Microsoft.AspNetCore.Mvc;
using TCC_Web_ERP.Helpers;

namespace TCC_Web_ERP.ViewComponents
{
    public class MenuViewComponent(MenuHelper menuHelper) : ViewComponent
    {
        private readonly MenuHelper _menuHelper = menuHelper;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menuHierarchy = await _menuHelper.GetMenuHierarchyAsync();
            return View(menuHierarchy);
        }
    }
}
