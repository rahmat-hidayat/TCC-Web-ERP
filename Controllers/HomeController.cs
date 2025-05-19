using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCC_Web_ERP.Data;
using TCC_Web_ERP.Models;

namespace TCC_Web_ERP.Controllers
{
    public class HomeController(AppDbContext context) : Controller
    {
        private readonly AppDbContext _context = context;

        public async Task<IActionResult> Index()
        {
            var menus = await _context.TMENU
                .Where(m => m.IsActive==true)
                .OrderBy(m => m.OrderNo)
                .ToListAsync();

            return View(menus);
        }
    }
}
