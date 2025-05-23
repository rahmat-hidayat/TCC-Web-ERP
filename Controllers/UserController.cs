using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using TCC_Web_ERP.Data;
using TCC_Web_ERP.Models;
using TCC_Web_ERP.ViewModels;
using System.Collections.Generic;
using BCrypt.Net;

namespace TCC_Web_ERP.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // ================================
        // GET: User Index (View + Filter)
        // ================================
        public IActionResult Index()
        {
            var roleList = new SelectList(
                _context.TROLE.Where(r => r.IsActive).ToList(),
                "RoleId",
                "RoleName"
            );

            var viewModel = new UserIndexViewModel
            {
                RoleList = roleList
            };

            return View(viewModel);
        }

        // ===========================================
        // GET: JSON User List for DataTables (Ajax)
        // ===========================================
        [HttpGet]
        public async Task<IActionResult> GetUsersJson()
        {
            try
            {
                var draw = HttpContext.Request.Query["draw"].FirstOrDefault() ?? "1";
                int.TryParse(HttpContext.Request.Query["start"].FirstOrDefault(), out int start);
                int.TryParse(HttpContext.Request.Query["length"].FirstOrDefault(), out int length);
                int.TryParse(HttpContext.Request.Query["order[0][column]"].FirstOrDefault(), out int sortColumnIndex);
                var sortDirection = HttpContext.Request.Query["order[0][dir]"].FirstOrDefault() ?? "asc";

                var searchValue = HttpContext.Request.Query["search[value]"].FirstOrDefault();
                var searchName = HttpContext.Request.Query["searchName"].FirstOrDefault();
                var roleFilter = HttpContext.Request.Query["roleFilter"].FirstOrDefault();

                var query = _context.TUSER
                    .Include(u => u.Role)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchName))
                {
                    var pattern = $"%{searchName}%";
                    query = query.Where(u => EF.Functions.Like(u.UserName, pattern));
                }
                else if (!string.IsNullOrEmpty(searchValue))
                {
                    var pattern = $"%{searchValue}%";
                    query = query.Where(u =>
                        EF.Functions.Like(u.UserName, pattern) ||
                        (u.Role != null && EF.Functions.Like(u.Role.RoleName, pattern))
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

                List<TUser> data;
                if (length == -1)
                {
                    // Jika user memilih "All", ambil semua data (tanpa paginasi)
                    data = await query.ToListAsync();
                }
                else
                {
                    // Ambil data sesuai paginasi normal
                    data = await query.Skip(start).Take(length).ToListAsync();
                }

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

        // ===============================
        // POST: Toggle User Status (Ajax)
        // ===============================
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

        // ==========================
        // POST: Create New User JSON
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateJson(TUser user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userName = HttpContext.Session.GetString("UserName") ?? "System";

                    if (!string.IsNullOrEmpty(user.UserPassword))
                    {
                        user.UserPassword = BCrypt.Net.BCrypt.HashPassword(user.UserPassword);
                    }

                    user.EntDate = DateTime.Now;
                    user.EntUser = userName;
                    user.UptUser = userName;
                    user.UptDate = DateTime.Now;
                    user.Status = "ACT";
                    user.Valid = DateTime.Now;
                    user.UptProgramm = "USER_CREATE";
                    user.Version = "1.1.7.6";

                    _context.TUSER.Add(user);
                    _context.SaveChanges();

                    return Ok(new { success = true });
                }
                catch (Exception ex)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Gagal menyimpan ke database: " + ex.Message
                    });
                }
            }

            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new
            {
                success = false,
                message = "Data tidak valid",
                errors
            });
        }

        // ===============================
        // Tambahan CRUD (Edit, Delete, dll)
        // ===============================
        // Tambahkan method EditJson, DeleteJson, atau Detail jika diperlukan
    }
}
