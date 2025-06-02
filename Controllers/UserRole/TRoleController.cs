using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCC_Web_ERP.Data;
using TCC_Web_ERP.Models;
using TCC_Web_ERP.ViewModels;

namespace TCC_Web_ERP.Controllers.UserRole
{
    public class TRoleController : Controller
    {
        private readonly AppDbContext _context;

        public TRoleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: TRole
        public IActionResult Index()
        {
            var roles = _context.TROLE
                .Where(r => r.IsActive)
                .OrderBy(r => r.RoleName)
                .Select(r => new { r.RoleId, r.RoleName })
                .ToList();

            var selectList = new SelectList(roles, "RoleId", "RoleName");

            var model = new RoleIndexViewModel
            {
                RoleList = selectList
            };

            return View(model);
        }

        // POST: TRole/DataTablesJson
        [HttpPost]
        public async Task<IActionResult> DataTablesJson()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault());
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault());

            var statusFilter = Request.Form["statusFilter"].FirstOrDefault();
            var searchName = Request.Form["searchName"].FirstOrDefault()?.ToLower();

            var query = _context.TROLE.AsQueryable();

            // Filter by status if provided
            if (!string.IsNullOrEmpty(statusFilter))
            {
                bool isActiveFilter = statusFilter == "Active";
                query = query.Where(r => r.IsActive == isActiveFilter);
            }

            // Filter by role name search
            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(r => r.RoleName != null && r.RoleName.ToLower().Contains(searchName));
            }

            var totalRecords = await _context.TROLE.CountAsync();
            var filteredRecords = await query.CountAsync();

            // Handle length = -1 (show all) and invalid lengths
            if (length == -1)
                length = filteredRecords;
            if (length <= 0)
                length = 10;

            var data = await query
                .OrderBy(r => r.RoleName)
                .Skip(start)
                .Take(length)
                .ToListAsync();

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = data.Select(r => new
                {
                    r.RoleId,
                    r.RoleName,
                    r.Description,
                    IsActive = r.IsActive ? "Active" : "Inactive"
                })
            };

            return Json(response);
        }

        // GET: TRole/Create
        public IActionResult Create()
        {
            return PartialView("_CreateEdit", new TRole { RoleName = string.Empty });
        }

        // POST: TRole/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoleName,Description,IsActive")] TRole role)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateEdit", role);
            }

            bool exists = await _context.TROLE.AnyAsync(r => r.RoleName == role.RoleName);
            if (exists)
            {
                ModelState.AddModelError("RoleName", "Nama Role sudah digunakan.");
                return PartialView("_CreateEdit", role);
            }

            try
            {
                if (!string.IsNullOrEmpty(role.RoleName))
                {
                    role.RoleName = role.RoleName.ToUpper();
                }

                if (!string.IsNullOrEmpty(role.Description))
                {
                    role.Description = role.Description.ToUpper();
                }
                _context.Add(role);
                             
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Role berhasil dibuat" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saat menyimpan: " + ex.Message });
            }
        }

        // GET: TRole/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var role = await _context.TROLE.FindAsync(id);
            if (role == null)
                return NotFound();

            return PartialView("_CreateEdit", role);
        }

        // POST: TRole/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoleId,RoleName,Description,IsActive")] TRole role)
        {
            if (id != role.RoleId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return PartialView("_CreateEdit", role);
            }

            bool exists = await _context.TROLE.AnyAsync(r => r.RoleName == role.RoleName && r.RoleId != role.RoleId);
            if (exists)
            {
                ModelState.AddModelError("RoleName", "Nama Role sudah digunakan.");
                return PartialView("_CreateEdit", role);
            }

            try
            {
                if (!string.IsNullOrEmpty(role.RoleName))
                {
                    role.RoleName = role.RoleName.ToUpper();
                }

                if (!string.IsNullOrEmpty(role.Description))
                {
                    role.Description = role.Description.ToUpper();
                }
                _context.Update(role);
                               
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Role berhasil diupdate" });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(role.RoleId)) return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saat update: " + ex.Message });
            }
        }

        // POST: TRole/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _context.TROLE.FindAsync(id);
            if (role == null)
                return Json(new { success = false, message = "Role tidak ditemukan" });

            try
            {
                _context.TROLE.Remove(role);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Role berhasil dihapus" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saat hapus: " + ex.Message });
            }
        }

        private bool RoleExists(int id)
        {
            return _context.TROLE.Any(e => e.RoleId == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            var role = await _context.TROLE.FindAsync(id);
            if (role == null)
                return Json(new { success = false, message = "Role tidak ditemukan" });

            try
            {
                role.IsActive = isActive;
                _context.Update(role);
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
