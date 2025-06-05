using Microsoft.EntityFrameworkCore;
using TCC_Web_ERP.Data;
using TCC_Web_ERP.Models;

namespace TCC_Web_ERP.Helpers
{
    public class MenuHelper
    {
        private readonly AppDbContext _context;

        // Konstruktor untuk menyuntikkan AppDbContext
        public MenuHelper(AppDbContext context)
        {
            _context = context;
        }

        // Mengambil hierarki menu (menu utama dan submenu)
        public async Task<List<MenuItem>> GetMenuHierarchyAsync()
        {
            // Ambil semua menu yang aktif dan urutkan berdasarkan OrderNo
            var menus = await _context.TMENU
                .Where(m => m.IsActive)  // Menyaring menu yang aktif
                .OrderBy(m => m.OrderNo)  // Mengurutkan berdasarkan OrderNo
                .ToListAsync();

            // Membentuk hierarki menu dengan submenu
            var menuHierarchy = menus
                .Where(m => m.ParentId == null)  // Menu utama yang tidak memiliki ParentId
                .Select(m => new MenuItem
                {
                    Menu = m,
                    Children = GetChildren(m.MenuId, menus)  // Mendapatkan submenu berdasarkan MenuId
                })
                .ToList();

            return menuHierarchy;
        }

        // Fungsi rekursif untuk mengambil submenu berdasarkan ParentId
        private static List<MenuItem> GetChildren(int parentId, List<TMenu> allMenus)
        {
            return allMenus
                .Where(m => m.ParentId == parentId)  // Menyaring menu berdasarkan ParentId
                .Select(m => new MenuItem
                {
                    Menu = m,
                    Children = GetChildren(m.MenuId, allMenus)  // Rekursif untuk submenu
                })
                .ToList();
        }
    }

    // Kelas MenuItem untuk merepresentasikan Menu dan Submenu
    public class MenuItem
    {
        public required TMenu Menu { get; set; }  // Menu utama yang wajib diinisialisasi
        public List<MenuItem> Children { get; set; } = new List<MenuItem>();  // Submenu
    }

}
