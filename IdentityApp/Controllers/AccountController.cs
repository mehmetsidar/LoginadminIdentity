using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using IdentityApp.Models;


namespace IdentityApp.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUser> _userManager;

        private RoleManager<AppRole> _roleManager;


        private SignInManager<AppUser> _singInManager;
        private IEmailSender _emailSender;

        public AccountController(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        SignInManager<AppUser> singInManager,
        IEmailSender emailSender
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _singInManager = singInManager;
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
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    await _singInManager.SignOutAsync();

                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError("", "Hesabınızı Onaylayınız");
                        return View(model);
                    }

                    var result = await _singInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);
                    if (result.Succeeded)
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);
                        await _userManager.SetLockoutEndDateAsync(user, null);

                        return RedirectToAction("Index", "Home");
                    }
                    else if (result.IsLockedOut)
                    {
                        var lockouDate = await _userManager.GetLockoutEndDateAsync(user);
                        var timeLeft = lockouDate.Value - DateTime.UtcNow;
                        ModelState.AddModelError("", $"Hesabınız Kitlendi, Lütfen{timeLeft.Minutes} dakika sonra deneyiniz");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Hatalı Parola");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Hatalı Email");
                }
            }
            return View();
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
                var user = new AppUser { UserName = model.UserName, Email = model.Email, PhoneNumber = model.PhoneNumber, FulName = model.FulName };
                IdentityResult result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var url = Url.Action("ConfirEmail", "Account", new { user.Id, token });

                    //Email
                    await _emailSender.SendEmailAsync(user.Email, "Hesap Onayı",
                    $"Lütfen Email Hesabınızı Onaylamak İçin Linke <a href='http://localhost:5011{url}'>Tıklayınız</a>");

                    TempData["message"] = "Email Hesabınızdaki Onay Mailine Tıklayın";
                    return RedirectToAction("Login", "Account");
                }
                foreach (IdentityError err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
            }
            return View();
        }

        public async Task<IActionResult> ConfirEmail(string Id, string token)
        {
            if (Id == null || token == null)
            {
                TempData["message"] = "Gecersiz Token";
                return View();
            }
            var user = await _userManager.FindByIdAsync(Id);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    TempData["message"] = "Hesabınız Onaylandı";
                    return RedirectToAction("Login", "Account");
                }
            }

            TempData["message"] = "Kullanıcı Bulunamadı";
            return View();

        }


        public async Task<IActionResult> Logout()
        {
            await _singInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        public async Task<IActionResult> ForgotPasswordWithEmail(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                TempData["message"] = "Eposta Adresinizi Giriniz";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                TempData["message"] = "Eposta Adresi ile eşleşen bir hesap bulunamadı";
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var url = Url.Action("ResetPassword", "Account", new { user.Id, token });
            await _emailSender.SendEmailAsync(Email, "Parolayı Sıfırlama", $"Parolanızı Yenilemek İçin Linke <a href='http://localhost:5011{url}'>Tıklayınız</a>.");
            TempData["message"] = "Eposta Adresinize Gönderilen Link ile şifrenizi Sıfırlayabilriisniz";
            return View();
        }

        


        // public IActionResult ResetPassword(string Id,string token)
        // {

        // }



    }
}