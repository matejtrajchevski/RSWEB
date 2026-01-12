using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityManagement.Data;
using UniversityManagement.Models;

namespace UniversityManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UniversityContext _context;

        public AccountController(UniversityContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users
                .Include(u => u.Teacher)
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);

                if (user.Role == "Professor" && user.TeacherId.HasValue)
                {
                    HttpContext.Session.SetInt32("TeacherId", user.TeacherId.Value);
                }
                else if (user.Role == "Student" && user.StudentId.HasValue)
                {
                    HttpContext.Session.SetString("StudentId", user.StudentId.Value.ToString());
                }

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid username or password";
            return View();
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
