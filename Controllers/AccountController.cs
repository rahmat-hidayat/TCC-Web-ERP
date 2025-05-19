using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TCC_Web_ERP.Data;
using TCC_Web_ERP.Models;
using Microsoft.AspNetCore.Http;

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

            string hashedPassword = ComputeSha512Hash(password);

            var user = await _context.TUSER
                .FirstOrDefaultAsync(u => u.UserName == username && u.UserPassword == hashedPassword && u.Status == "ACT");

            if (user == null)
            {
                ViewBag.Error = "Username atau password salah, atau user tidak aktif.";
                return View();
            }

            // Simpan session user login
            // Simpan session user login
            if (!string.IsNullOrEmpty(user.UserName))
            {
                HttpContext.Session.SetString("UserName", user.UserName);
            }
            // Updated code to handle potential null reference for 'user.UserName'
            if (!string.IsNullOrEmpty(user.UserName))
            {
                HttpContext.Session.SetString("UserName", user.UserName);
            }
            else
            {
                HttpContext.Session.SetString("UserName", string.Empty); // Fallback to an empty string
            }
            // Updated code to handle potential null reference for 'user.UserName'
            if (!string.IsNullOrEmpty(user.UserName))
            {
                HttpContext.Session.SetString("UserName", user.UserName);
            }
            else
            {
                HttpContext.Session.SetString("UserName", string.Empty); // Fallback to an empty string
            }
            // Updated code to handle potential null reference for 'user.UserName'
            if (!string.IsNullOrEmpty(user.UserName))
            {
                HttpContext.Session.SetString("UserName", user.UserName);
            }
            else
            {
                HttpContext.Session.SetString("UserName", string.Empty); // Fallback to an empty string
            }
            // Updated code to handle potential null reference for 'user.UserName'
            if (!string.IsNullOrEmpty(user.UserName))
            {
                HttpContext.Session.SetString("UserName", user.UserName);
            }
            else
            {
                HttpContext.Session.SetString("UserName", string.Empty); // Fallback to an empty string
            }
            // Updated code to handle potential null reference for 'user.UserName'
            if (!string.IsNullOrEmpty(user.UserName))
            {
                HttpContext.Session.SetString("UserName", user.UserName);
            }
            else
            {
                HttpContext.Session.SetString("UserName", string.Empty); // Fallback to an empty string
            }


            // Redirect ke halaman dashboard (atau home)
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Bersihkan semua session
            HttpContext.Session.Clear();

            // Redirect ke halaman login setelah logout
            return RedirectToAction("Login");
        }

        public static string ComputeSha512Hash(string rawData)
        {
            byte[] bytes = SHA512.HashData(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new();
            foreach (var b in bytes)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
}
