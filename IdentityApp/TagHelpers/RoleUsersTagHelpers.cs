using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
namespace IdentityApp.TagHelpers
{
    [HtmlTargetElement("td", Attributes = "asp-role-users")]
    public class RoleUsersTagHelpers : TagHelper
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        public RoleUsersTagHelpers(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HtmlAttributeName("asp-role-users")]
        public string RoleId { get; set; } = null!;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var userName = new List<string>();
            var role = await _roleManager.FindByIdAsync(RoleId);
            if (role != null && role.Name != null)
            {
                foreach (var user in _userManager.Users)
                {
                    if (await _userManager.IsInRoleAsync(user, role.Name))
                    {
                        userName.Add(user.UserName ?? "");
                    }
                }
                output.Content.SetHtmlContent(userName.Count == 0 ? "Kullanıcı Yok" : setHtml(userName));
            }
        }

        private string setHtml(List<string> userName)
        {
            var html = "<ul>";
            foreach (var item in userName)
            {
                html += "<li>" + item + "</li>";
            }

            html += "</ul>";

            return html;
        }
    }
}