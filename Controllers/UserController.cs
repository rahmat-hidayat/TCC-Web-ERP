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
    public class UserController(AppDbContext context) : Controller
    {
        private readonly AppDbContext _context = context;

        // GET: User
        public IActionResult Index()
        {
            ViewBag.RoleList = new SelectList(_context.TROLE.Where(r => r.IsActive).ToList(), "RoleId", "RoleName");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersJson()
        {
            try
            {
                var draw = HttpContext.Request.Query["draw"].FirstOrDefault() ?? "1";

                if (!int.TryParse(HttpContext.Request.Query["start"].FirstOrDefault(), out int start))
                    start = 0;
                if (!int.TryParse(HttpContext.Request.Query["length"].FirstOrDefault(), out int length))
                    length = 10;

                if (!int.TryParse(HttpContext.Request.Query["order[0][column]"].FirstOrDefault(), out int sortColumnIndex))
                    sortColumnIndex = 1;

                var sortDirection = HttpContext.Request.Query["order[0][dir]"].FirstOrDefault() ?? "asc";
                var searchValue = HttpContext.Request.Query["search[value]"].FirstOrDefault()?.ToLower();
                var searchName = HttpContext.Request.Query["searchName"].FirstOrDefault()?.ToLower();
                var roleFilter = HttpContext.Request.Query["roleFilter"].FirstOrDefault();

                var query = _context.TUSER.Include(u => u.Role).AsQueryable();

                if (!string.IsNullOrEmpty(searchName))
                {
                    query = query.Where(u => u.UserName != null && u.UserName.ToLower().Contains(searchName));
                }
                else if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(u =>
                        (u.UserName != null && u.UserName.ToLower().Contains(searchValue)) ||
                        (u.Role != null && u.Role.RoleName != null && u.Role.RoleName.ToLower().Contains(searchValue))
                    );
                }

                if (!string.IsNullOrEmpty(roleFilter) && int.TryParse(roleFilter, out int roleId))
                {
                    query = query.Where(u => u.RoleId == roleId);
                }

                var recordsTotal = await _context.TUSER.CountAsync();
                var recordsFiltered = await query.CountAsync();

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

                if (start < 0) start = 0;
                if (length <= 0) length = 10;

                var data = await query.Skip(start).Take(length).ToListAsync();

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
                return StatusCode(500, new { error = "Terjadi error di server: " + ex.Message });
            }
        }

        // Method untuk toggle status via AJAX POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatusJson([FromForm] int id)
        {
            var user = await _context.TUSER.FindAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User tidak ditemukan" });
            }

            user.Status = user.Status == "ACT" ? "INA" : "ACT";
            user.UptDate = DateTime.Now;
            user.UptUser = User.Identity?.Name ?? "system";

            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    status = user.Status == "ACT" ? "Aktif" : "Nonaktif"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Gagal update status: " + ex.Message });
            }
        }

        // CRUD method lain bisa tetap sama seperti sebelumnya
    }
}
