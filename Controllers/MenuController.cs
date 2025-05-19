using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCC_Web_ERP.Data;
using TCC_Web_ERP.Models;

namespace TCC_Web_ERP.Controllers
{
    public class MenuController(AppDbContext context) : Controller
    {
        private readonly AppDbContext _context = context;

        // Mendapatkan menu dinamis berdasarkan role user
        public async Task<IActionResult> UserMenu()
        {
            // Ambil username dari session/claims
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return Unauthorized();

            // Ambil user beserta role-nya
            var user = await _context.TUSER
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null || user.RoleId == null)
                return NotFound();

            // Ambil menu yang visible untuk role user
            var menus = await _context.ROLE_MENU
    .Where(rm => rm.RoleId == user.RoleId && rm.IsVisible)
    .Include(rm => rm.Menu)
    .Select(rm => rm.Menu)
    .OrderBy(m => m.OrderNo) // properti urutan di TMenu adalah OrderNo
    .ToListAsync();

            return PartialView("_UserMenuPartial", menus);
        }
    }
    
   
}
