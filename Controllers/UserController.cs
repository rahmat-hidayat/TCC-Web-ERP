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
                    2 => sortDirection == "asc" ? query.OrderBy(u => u.UptProgramm) : query.OrderByDescending(u => u.UptProgramm),
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
                    uptProgram = u.UptProgramm,
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
                    status = user.Status == "ACT" ? "AKTIF" : "NON AKTIF"
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
        public IActionResult CreateJson(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tambahkan validasi duplikat username
                if (_context.TUSER.Any(u => u.UserName == model.UserName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Username sudah digunakan. Silakan pilih username lain."
                    });
                }

                try
                {
                    var userName = HttpContext.Session.GetString("UserName") ?? "System";

                    var user = new TUser
                    {
                        UserName = model.UserName.ToUpper(),
                        UserPassword = BCrypt.Net.BCrypt.HashPassword(model.UserPassword),
                        RoleId = model.RoleId,

                        // Diisi manual
                        EntDate = DateTime.Now,
                        EntUser = userName.ToUpper(),
                        UptUser = userName.ToUpper(),
                        UptDate = DateTime.Now,
                        Status = "ACT",
                        Valid = DateTime.Now.AddYears(50),
                        ChangePass = "0",
                        UptProgramm = "USER_CREATE".ToUpper(),
                        Remark = "USER BARU".ToUpper(),
                        Version = "1.1.7.6",
                        GroupId = 1,
                        Blocked = 0,
                        MacAdd = null,
                        Tablet = 0,
                        Driver = 0,
                        SuperUser = 0,
                        Email = (model.UserName ?? "").ToUpper() + "@GMAIL.COM",
                        Esign = null
                    };

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

        // ==================================
        // GET: Partial View Add User Modal
        // ==================================
        [HttpGet]
        public IActionResult GetAddUserModal()
        {
            var model = new CreateUserViewModel
            {
                RoleList = _context.TROLE
                    .Where(r => r.IsActive)
                    .Select(r => new SelectListItem
                    {
                        Value = r.RoleId.ToString(),
                        Text = r.RoleName
                    }).ToList()
            };

            return PartialView("_AddUserModal", model);
        }


        // ===========================================
        // GET: JSON Ambil data user berdasarkan ID (untuk Edit Modal)
        // ===========================================
        [HttpGet]
        public async Task<IActionResult> GetUserByIdJson(int id)
        {
            var user = await _context.TUSER
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound(new { message = "User tidak ditemukan" });

            return Json(new
            {
                userId = user.UserId,
                userName = user.UserName,
                roleId = user.RoleId,
                uptProgramm = user.UptProgramm,
                status = user.Status,
                entUser = user.EntUser,
                entDate = user.EntDate?.ToString("yyyy-MM-dd"),
                // tambahkan properti lain jika perlu
            });
        }


        // ===========================================
        // GET: User/Edit/5 - Load data user untuk modal edit
        // ===========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.TUSER.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                UserId = user.UserId,
                UserName = user.UserName,
                RoleId = user.RoleId ?? 0,
                RoleList = await GetRoleSelectListAsync()
            };

            return PartialView("_EditUserModal", model);
        }

        // ===========================================
        // POST: User/Edit/5 - Update data user dari modal edit (Ajax)
        // ===========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.RoleList = await GetRoleSelectListAsync();
                return PartialView("_EditUserModal", model);
            }

            var user = await _context.TUSER.FindAsync(model.UserId);
            if (user == null)
            {
                return Json(new { success = false, message = "User tidak ditemukan." });
            }

            // Update properti-properti yang diizinkan
            user.UserName = ToUpperSafe(model.UserName);
            user.RoleId = model.RoleId == 0 ? null : model.RoleId;
            user.UptDate = DateTime.Now;
            user.UptUser = GetCurrentUserName();
            user.UptProgramm = "USER EDIT";
            user.Remark = "USER EDIT";

            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "User berhasil diperbarui." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Gagal update user: {ex.Message}" });
            }
        }

        // ===========================================
        // Helper: Konversi string ke huruf besar, aman dari null
        // ===========================================
        private static string ToUpperSafe(string? input)
        {
            return (input ?? string.Empty).Trim().ToUpper();
        }

        // ===========================================
        // Helper: Ambil nama user login dalam huruf besar, fallback ke "SYSTEM"
        // ===========================================
        private string GetCurrentUserName()
        {
            return User.Identity?.Name?.ToUpper() ?? "SYSTEM";
        }


        // ===========================================
        // Helper: Ambil daftar role sebagai SelectListItem
        // ===========================================
        private async Task<List<SelectListItem>> GetRoleSelectListAsync()
        {
            return await _context.TROLE
                .Where(r => r.IsActive)
                .OrderBy(r => r.RoleName)
                .Select(r => new SelectListItem
                {
                    Value = r.RoleId.ToString(),
                    Text = r.RoleName
                })
                .ToListAsync();
        }

        // ===========================================
        // Ambil daftar role untuk modal edit
        // ===========================================
        [HttpGet]
        public async Task<IActionResult> GetRoleListJson()
        {
            var roles = await _context.TROLE
                .Where(r => r.IsActive)
                .OrderBy(r => r.RoleName)
                .Select(r => new
                {
                    value = r.RoleId,
                    text = r.RoleName
                })
                .ToListAsync();

            return Json(roles);
        }

        // ================================
        // GetUserDetail
        // ================================
        [HttpGet]
        public async Task<IActionResult> GetUserDetail(int id)
        {
            var user = await _context.TUSER
                .Include(u => u.Role)
                .Where(u => u.UserId == id)
                .Select(u => new UserDetailViewModel
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    GroupId = u.GroupId,
                    UserPassword = u.UserPassword,
                    LastLogin = u.LastLogin,
                    EntUser = u.EntUser,
                    EntDate = u.EntDate,
                    UptUser = u.UptUser,
                    UptDate = u.UptDate,
                    UptProgramm = u.UptProgramm,
                    Remark = u.Remark,
                    Version = u.Version,
                    Status = u.Status,
                    Valid = u.Valid,
                    ChangePass = u.ChangePass,
                    Blocked = u.Blocked,
                    MacAdd = u.MacAdd,
                    Email = u.Email,
                    SuperUser = u.SuperUser,
                    Tablet = u.Tablet,
                    Driver = u.Driver,
                    RoleId = u.RoleId,
                    RoleName = u.Role != null ? u.Role.RoleName : null
                }).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return PartialView("_DetailUserModal", user);
        }

    }
}
