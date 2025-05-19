using Microsoft.EntityFrameworkCore;
using TCC_Web_ERP.Data;
using TCC_Web_ERP.Models;

namespace TCC_Web_ERP.Helpers
{
    public class MenuHelper(AppDbContext context)
    {
        private readonly AppDbContext _context = context;

        // Updated GetMenuHierarchyAsync method
        public async Task<List<MenuItem>> GetMenuHierarchyAsync()
        {
            var users = await _context.TUSER.ToListAsync();
            var menus = await _context.TMENU
                .Where(m => m.IsActive == true)
                .OrderBy(m => m.OrderNo)
                .ToListAsync();

            var menuHierarchy = menus
                .Where(m => m.ParentId == null)
                .Select(m => new MenuItem
                {
                    Menu = m,
                    Children = GetChildren(m.MenuId, menus)
                })
                .ToList();

            return menuHierarchy;
        }

        // Fungsi rekursif untuk mengambil child menu
        private static List<MenuItem> GetChildren(int parentId, List<TMenu> allMenus)
        {
            return [
                ..allMenus
                    .Where(m => m.ParentId == parentId)
                    .Select(m => new MenuItem
                    {
                        Menu = m,
                        Children = GetChildren(m.MenuId, allMenus)
                    })
            ];
        }
    }

    // Updated MenuItem class
    public class MenuItem
    {
        public TMenu Menu { get; set; } = null!;
        public List<MenuItem> Children { get; set; } = [];
    }
}
