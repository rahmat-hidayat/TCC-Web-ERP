using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCC_Web_ERP.Data;
using TCC_Web_ERP.Models;
using Microsoft.AspNetCore.Http;
using System;

namespace TCC_Web_ERP.Controllers
{
    public class AccountController(AppDbContext context) : Controller
    {
        private readonly AppDbContext _context = context;

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Username dan password harus diisi.";
                return View();
            }

            // Ambil user berdasarkan username dan status aktif
            var user = await _context.TUSER
                .FirstOrDefaultAsync(u => u.UserName == username && u.Status == "ACT");

            if (user == null || string.IsNullOrEmpty(user.UserPassword))
            {
                ViewBag.Error = "Username atau password salah, atau user tidak aktif.";
                return View();
            }

            // Verifikasi password menggunakan bcrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.UserPassword);

            if (!isPasswordValid)
            {
                ViewBag.Error = "Username atau password salah.";
                return View();
            }

            // ✅ Update kolom login info
            user.LastLogin = DateTime.Today; // format: yyyy-MM-dd 00:00:00.000
            user.UptProgramm = "LOGIN TO SYSTEM";
            user.Version = "1.1.7.6";

            await _context.SaveChangesAsync();

            // Simpan session user login
            HttpContext.Session.SetString("UserName", user.UserName ?? string.Empty);

            // Redirect ke halaman dashboard
            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
