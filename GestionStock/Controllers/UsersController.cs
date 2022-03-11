#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionStock.Data;
using GestionStock.Models;
using System.Text;
using GestionStock.Models.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace GestionStock.Controllers
{
    public class UsersController : Controller
    {
        private readonly ACTEAM_StockContext _context;

        public UsersController(ACTEAM_StockContext context)
        {
            _context = context;
        }
        // GET: Users/Create
        public IActionResult Register()
        {
            return View();
        }
        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Id,LastName,FirstName,Email,Phone,Password,IsAdmin,Salt")] User user)
        {
            if (ModelState.IsValid)
            {
                var userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                if(userFromDb == null)
                {
                    if(user.IsAdmin == null)
                    {
                        user.IsAdmin = false;
                    }
                    byte[] saltTemp = PasswordHasher.GenerateSalt();
                    user.Salt = Convert.ToBase64String(saltTemp);
                    user.Password = Convert.ToBase64String(PasswordHasher.HashPasswordWithSalt(Encoding.UTF8.GetBytes(user.Password), saltTemp));
                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Items");
                }
            }
            return View(user);
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //Login With Credentials
        public async Task<IActionResult> Login([Bind("Email,Password")] Credential user)
        {
            if (ModelState.IsValid)
            {
                var userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (userFromDb == null)
                {
                    ModelState.AddModelError("", "Email or Password is incorrect");
                    return View(user);
                }
                if (!PasswordHasher.VerifyPasswordWithSalt(Convert.FromBase64String(userFromDb.Password), userFromDb.Salt, user.Password))
                {
                    ModelState.AddModelError("", "Email or Password is incorrect");
                    return View(user);
                }
                if (userFromDb.IsAdmin == true)
                {
                    List<Claim> claims = new List<Claim>()
                        {
                            new Claim(ClaimTypes.Email, userFromDb.Email),
                            new Claim(ClaimTypes.NameIdentifier, userFromDb.Id.ToString()),
                            new Claim("Administrator", "true")
                        };
                    ClaimsIdentity identity = new ClaimsIdentity(claims, "AuthCookie");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync("AuthCookie", claimsPrincipal);
                    return RedirectToAction("Index", "Items");
                }
                else
                {
                    List<Claim> claims = new List<Claim>()
                        {
                            new Claim(ClaimTypes.Email, userFromDb.Email),
                            new Claim(ClaimTypes.NameIdentifier, userFromDb.Id.ToString()),
                        };
                    ClaimsIdentity identity = new ClaimsIdentity(claims, "AuthCookie");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync("AuthCookie", claimsPrincipal);
                    return RedirectToAction("Index", "Items");
                }
            }
            return View(user);
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AuthCookie");
            return RedirectToAction("Index", "Items");
        }
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
