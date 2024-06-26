using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using IdentityApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace IdentityApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            SignInManager<AppUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var users = await _userManager.Users.Where(u => u.Email == model.Email).ToListAsync();

                if (users.Count > 1)
                {
                    ModelState.AddModelError("", "Aynı e-posta adresine sahip birden fazla kullanıcı bulundu.");
                    return View(model);
                }

                var user = users.SingleOrDefault();

                if (user != null)
                {
                    await _signInManager.SignOutAsync();

                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError("", "Hesabınızı Onaylayınız");
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);
                    if (result.Succeeded)
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);
                        await _userManager.SetLockoutEndDateAsync(user, null);

                        return RedirectToAction("Index", "Home");
                    }
                    else if (result.IsLockedOut)
                    {
                        var lockoutDate = await _userManager.GetLockoutEndDateAsync(user);
                        var timeLeft = lockoutDate.Value - DateTime.UtcNow;
                        ModelState.AddModelError("", $"Hesabınız kilitlendi, lütfen {timeLeft.Minutes} dakika sonra tekrar deneyin.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Hatalı parola");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Hatalı e-posta");
                }
            }
            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    FulName = model.FulName
                };
                IdentityResult result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var url = Url.Action("ConfirmEmail", "Account", new { user.Id, token });

                    // Email
                    await _emailSender.SendEmailAsync(user.Email, "Hesap Onayı",
                        $"Lütfen e-posta hesabınızı onaylamak için <a href='http://localhost:5011{url}'>tıklayınız</a>.");

                    TempData["message"] = "E-posta hesabınızdaki onay mailine tıklayın.";
                    return RedirectToAction("Login", "Account");
                }
                foreach (IdentityError err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> ConfirmEmail(string Id, string token)
        {
            if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(token))
            {
                TempData["message"] = "Geçersiz token";
                return View();
            }
            var user = await _userManager.FindByIdAsync(Id);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    TempData["message"] = "Hesabınız onaylandı.";
                    return RedirectToAction("Login", "Account");
                }
            }

            TempData["message"] = "Kullanıcı bulunamadı.";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPasswordWithEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["message"] = "E-posta adresinizi giriniz.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["message"] = "E-posta adresi ile eşleşen bir hesap bulunamadı.";
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var url = Url.Action("ResetPassword", "Account", new { user.Id, token });
            await _emailSender.SendEmailAsync(email, "Parolayı Sıfırlama", $"Parolanızı yenilemek için <a href='http://localhost:5011{url}'>tıklayınız</a>.");
            TempData["message"] = "E-posta adresinize gönderilen link ile şifrenizi sıfırlayabilirsiniz.";
            return View();
        }
    }
}
