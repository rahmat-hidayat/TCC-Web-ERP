using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCC_Web_ERP.Data;
using TCC_Web_ERP.Models;

namespace TCC_Web_ERP.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // GET: User - hanya load view tanpa data
        public IActionResult Index()
        {
            // Ambil list role aktif untuk dropdown filter di view
            ViewBag.RoleList = new SelectList(_context.TROLE.Where(r => r.IsActive).ToList(), "RoleId", "RoleName");
            return View();
        }

        // GET: Get users data untuk DataTables server-side processing
        [HttpGet]
        public async Task<IActionResult> GetUsersJson()
        {
            try
            {
                // Ambil parameter dari request DataTables, dengan validasi default agar tidak null
                var draw = HttpContext.Request.Query["draw"].FirstOrDefault() ?? "1";

                int start = 0;
                int length = 10; // default 10 data per page

                // Parsing parameter paging dengan TryParse agar aman dari error
                if (!int.TryParse(HttpContext.Request.Query["start"].FirstOrDefault(), out start))
                    start = 0;
                if (!int.TryParse(HttpContext.Request.Query["length"].FirstOrDefault(), out length))
                    length = 10;

                // Sorting column index dan direction
                int sortColumnIndex = 1; // default sort kolom ke-1 (UserName)
                string sortDirection = "asc";

                if (!int.TryParse(HttpContext.Request.Query["order[0][column]"].FirstOrDefault(), out sortColumnIndex))
                    sortColumnIndex = 1;

                sortDirection = HttpContext.Request.Query["order[0][dir]"].FirstOrDefault() ?? "asc";

                // Search/filter text untuk UserName dan RoleId
                var searchValue = HttpContext.Request.Query["search[value]"].FirstOrDefault()?.ToLower();

                // Custom filter: nama dan role dari parameter tersendiri (bisa disesuaikan)
                var searchName = HttpContext.Request.Query["searchName"].FirstOrDefault()?.ToLower();
                var roleFilter = HttpContext.Request.Query["roleFilter"].FirstOrDefault();

                // Query dasar join ke role
                var query = _context.TUSER.Include(u => u.Role).AsQueryable();

                // Filter berdasarkan nama (bisa gunakan searchName atau searchValue)
                // Asumsikan database collation case-insensitive, jadi tidak perlu ToLower()
                if (!string.IsNullOrEmpty(searchName))
                {
                    query = query.Where(u => u.UserName != null && u.UserName.Contains(searchName));
                }
                else if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(u =>
                        (u.UserName != null && u.UserName.Contains(searchValue)) ||
                        (u.Role != null && u.Role.RoleName != null &&
                         u.Role.RoleName.Contains(searchValue))
                    );
                }

                // Filter berdasarkan RoleId jika ada dan valid
                if (!string.IsNullOrEmpty(roleFilter) && int.TryParse(roleFilter, out int roleId))
                {
                    query = query.Where(u => u.RoleId == roleId);
                }

                // Total records sebelum filter
                var recordsTotal = await _context.TUSER.CountAsync();

                // Total records setelah filter
                var recordsFiltered = await query.CountAsync();

                // Sort berdasarkan kolom yang diklik DataTables (index 0-based)
                query = sortColumnIndex switch
                {
                    0 => sortDirection == "asc" ? query.OrderBy(u => u.UserId) : query.OrderByDescending(u => u.UserId),
                    1 => sortDirection == "asc" ? query.OrderBy(u => u.UserName) : query.OrderByDescending(u => u.UserName),
                    2 => sortDirection == "asc" ? query.OrderBy(u => u.GroupId) : query.OrderByDescending(u => u.GroupId),
                    3 => sortDirection == "asc" ? query.OrderBy(u => u.EntUser) : query.OrderByDescending(u => u.EntUser),
                    4 => sortDirection == "asc" ? query.OrderBy(u => u.EntDate) : query.OrderByDescending(u => u.EntDate),
                    5 => sortDirection == "asc" ? query.OrderBy(u => u.Status) : query.OrderByDescending(u => u.Status),
                    6 => sortDirection == "asc" ? query.OrderBy(u => u.RoleId) : query.OrderByDescending(u => u.RoleId),
                    7 => sortDirection == "asc"
                        ? query.OrderBy(u => u.Role != null ? u.Role.RoleName : "")
                        : query.OrderByDescending(u => u.Role != null ? u.Role.RoleName : ""),
                    _ => query.OrderBy(u => u.UserName),
                };

                // Ambil data sesuai paging, jangan ambil data negatif atau length 0
                if (start < 0) start = 0;
                if (length <= 0) length = 10;

                var data = await query.Skip(start).Take(length).ToListAsync();

                // Bentuk data yang akan dikirim ke DataTables
                var result = data.Select(u => new
                {
                    userId = u.UserId,
                    userName = u.UserName,
                    groupId = u.GroupId,
                    entUser = u.EntUser,
                    entDate = u.EntDate?.ToString("yyyy-MM-dd") ?? "",
                    status = u.Status,
                    roleId = u.RoleId,
                    roleName = u.Role?.RoleName ?? ""
                });

                // Return JSON sesuai format DataTables
                return Json(new
                {
                    draw,
                    recordsTotal,
                    recordsFiltered,
                    data = result
                });
            }
            catch (Exception ex)
            {
                // Return error detail supaya bisa debugging Ajax 500
                return StatusCode(500, new { error = "Terjadi error di server: " + ex.Message });
            }
        }

        // POST: Toggle user status
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.TUSER.FindAsync(id);
            if (user == null) return NotFound();

            user.Status = user.Status == "ACT" ? "INA" : "ACT";
            user.UptDate = DateTime.Now;
            user.UptUser = User.Identity?.Name ?? "system";

            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ... POST, GET Create/Edit/Delete tetap seperti sebelumnya (bisa dipertahankan) ...
    }
}
