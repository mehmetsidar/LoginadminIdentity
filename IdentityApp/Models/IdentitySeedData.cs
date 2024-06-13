using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityApp.Models
{
    public class IdentitySeedData
    {
        private const string adminUser = "Admin";
        private const string adminPassword = "Admin_123";
        public static async void IdentityTestUser(IApplicationBuilder app)
        {
            var context = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<IdentityContext>();
            if (context.Database.GetAppliedMigrations().Any())
            {
                context.Database.Migrate();
            }

            var userManager=app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            var user= await userManager.FindByNameAsync(adminUser);

            if(user==null){
                user=new AppUser{
                    UserName=adminUser,
                    FulName="Sidar",
                    Email="mehmetsidar@gmail.com",
                    PhoneNumber="05438729660"
                };

                await userManager.CreateAsync(user,adminPassword);
            }
        }

    }
}