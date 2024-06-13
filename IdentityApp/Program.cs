using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
//mail service
builder.Services.AddScoped<IEmailSender,SmtpEmailSender>(i=> new SmtpEmailSender(
builder.Configuration["EmailSender:Host"],
builder.Configuration.GetValue<int>("EmailSender:Port"),
builder.Configuration.GetValue<bool>("EmailSender:EnableSSL"),
builder.Configuration["EmailSender:Username"],
builder.Configuration["EmailSender:Password"]
));
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<IdentityContext>(Options => Options.UseSqlite(builder.Configuration["ConnectionStrings:SQLite_Connection"]));
builder.Services.AddIdentity<AppUser, AppRole>().AddEntityFrameworkStores<IdentityContext>().AddDefaultTokenProviders();

// // //Bu kısım password ve mail ayarların yapıldığı kısımdır

builder.Services.Configure<IdentityOptions>(Options =>
{
    // Options.Password.RequiredLength = 5;
    // Options.Password.RequireNonAlphanumeric = false;
    // Options.Password.RequireLowercase = false;
    // Options.Password.RequireUppercase = false;
    // Options.Password.RequireDigit = false;
    // Options.User.RequireUniqueEmail = true;
    // Options.Lockout.MaxFailedAccessAttempts = 3;
    // Options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);


    // Options.User.RequireUniqueEmail = true;

    // Options.Lockout.DefaultLockoutTimeSpan=TimeSpan.FromMinutes(5);
    // Options.Lockout.MaxFailedAccessAttempts=5;

    // //telefon email onay kodu
    Options.SignIn.RequireConfirmedEmail=true;
    //Options.SignIn.RequireConfirmedPhoneNumber=true;

});

//Login Kısımların Ayarını Yapiyoruz
builder.Services.ConfigureApplicationCookie(options=>{
    options.LoginPath="/Account/Login";
    options.AccessDeniedPath="/Account/AccessDenied";
    options.SlidingExpiration=true;
    options.ExpireTimeSpan=TimeSpan.FromDays(30);
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

IdentitySeedData.IdentityTestUser(app);
app.Run();
