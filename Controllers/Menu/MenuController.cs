using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCC_Web_ERP.Data;
using TCC_Web_ERP.Models;
using TCC_Web_ERP.ViewModels;

namespace TCC_Web_ERP.Controllers.Menu
{
    public class MenuController : Controller
    {
        private readonly AppDbContext _context;

        public MenuController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Menu
        public IActionResult Index()
        {
            var menus = _context.TMENU
                .Where(m => m.IsActive)  // Menampilkan menu yang aktif
                .OrderBy(m => m.Title)  // Urutkan berdasarkan Title
                .ToList();

            return View(menus);
        }

        // POST: Menu/DataTablesJson
        [HttpPost]
        public async Task<IActionResult> DataTablesJson()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault());
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault());

            var searchTitle = Request.Form["searchTitle"].FirstOrDefault()?.ToLower();
            var statusFilter = Request.Form["statusFilter"].FirstOrDefault();

            var query = _context.TMENU.AsQueryable();

            // Filter berdasarkan status jika disediakan
            if (!string.IsNullOrEmpty(statusFilter))
            {
                bool isActiveFilter = statusFilter == "Active";
                query = query.Where(m => m.IsActive == isActiveFilter);
            }

            // Filter berdasarkan title jika ada pencarian
            if (!string.IsNullOrEmpty(searchTitle))
            {
                // Menggunakan null-coalescing atau null-conditional operator untuk menangani null
                query = query.Where(m => (m.Title ?? "").ToLower().Contains(searchTitle)); // Menggunakan null-coalescing untuk menangani null
            }

            var totalRecords = await _context.TMENU.CountAsync();
            var filteredRecords = await query.CountAsync();

            // Handle length = -1 (show all) and invalid lengths
            if (length == -1)
                length = filteredRecords;
            if (length <= 0)
                length = 10;

            var data = await query
                .OrderBy(m => m.Title)
                .Skip(start)
                .Take(length)
                .ToListAsync();

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = data.Select(m => new
                {
                    m.MenuId,
                    m.Title,
                    m.Url,
                    m.ActionName,      // Menambahkan ActionName
                    m.ControllerName,  // Menambahkan ControllerName
                    m.Icon,            // Menambahkan Icon
                    m.ParentId,        // Menambahkan ParentId
                    m.OrderNo,         // Menambahkan OrderNo
                    IsActive = m.IsActive ? "Active" : "Inactive"
                })
            };

            return Json(response);
        }

        // GET: Menu/Create
        public IActionResult Create()
        {
            var menu = new TMenu();
            ViewData["ModalTitle"] = "Tambah Menu Baru";  // Set modal title untuk Create
            return PartialView("_MenuModal", menu);  // Kembalikan partial view untuk modal create
        }

        // POST: Menu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,ActionName,ControllerName,Icon,Url,ParentId,OrderNo,IsActive")] TMenu menu)
        {
            if (!ModelState.IsValid)
            {
                return View(menu);
            }

            // Cek apakah menu sudah ada berdasarkan Title
            bool exists = await _context.TMENU.AnyAsync(m => m.Title == menu.Title);
            if (exists)
            {
                ModelState.AddModelError("Title", "Nama menu sudah digunakan.");
                return View(menu);
            }

            try
            {
                // Memastikan data format yang benar
                if (!string.IsNullOrEmpty(menu.Title))
                {
                    menu.Title = menu.Title.ToUpper();
                }

                _context.Add(menu);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Menu berhasil dibuat" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saat menyimpan: " + ex.Message });
            }
        }

        // GET: Menu/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var menu = await _context.TMENU.FindAsync(id);
            if (menu == null)
                return NotFound();

            ViewData["ModalTitle"] = "Edit Menu";  // Set modal title untuk Edit
            return PartialView("_MenuModal", menu);  // Kembalikan partial view untuk modal edit
        }

        // POST: Menu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MenuId,Title,ActionName,ControllerName,Icon,Url,ParentId,OrderNo,IsActive")] TMenu menu)
        {
            if (id != menu.MenuId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return View(menu);
            }

            bool exists = await _context.TMENU.AnyAsync(m => m.Title == menu.Title && m.MenuId != menu.MenuId);
            if (exists)
            {
                ModelState.AddModelError("Title", "Nama menu sudah digunakan.");
                return View(menu);
            }

            try
            {
                // Memastikan data format yang benar
                if (!string.IsNullOrEmpty(menu.Title))
                {
                    menu.Title = menu.Title.ToUpper();
                }

                _context.Update(menu);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Menu berhasil diupdate" });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuExists(menu.MenuId))
                    return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saat update: " + ex.Message });
            }
        }

        // POST: Menu/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var menu = await _context.TMENU.FindAsync(id);
            if (menu == null)
                return Json(new { success = false, message = "Menu tidak ditemukan" });

            try
            {
                _context.TMENU.Remove(menu);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Menu berhasil dihapus" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saat hapus: " + ex.Message });
            }
        }

        private bool MenuExists(int id)
        {
            return _context.TMENU.Any(e => e.MenuId == id);
        }

        // POST: Menu/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            var menu = await _context.TMENU.FindAsync(id);
            if (menu == null)
                return Json(new { success = false, message = "Menu tidak ditemukan" });

            try
            {
                // Update status menu
                menu.IsActive = isActive;
                _context.Update(menu);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Gagal mengubah status: " + ex.Message });
            }
        }
    }
}
