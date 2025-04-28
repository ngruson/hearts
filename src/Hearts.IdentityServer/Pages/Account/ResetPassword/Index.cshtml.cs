using Hearts.IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hearts.IdentityServer.Pages.Account.ResetPassword;

[AllowAnonymous]
public class IndexModel(UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public bool PasswordReset { get; set; } = false;

    public IActionResult OnGetAsync(string email, string token)
    {
        this.Input = new InputModel
        {
            Email = email,
            Token = token
        };

        return this.Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (this.ModelState.IsValid)
        {
            ApplicationUser? user = await userManager.FindByEmailAsync(this.Input.Email);
            if (user == null)
            {
                this.ModelState.AddModelError("", "User not found");
                return this.Page();
            }

            IdentityResult result = await userManager.ResetPasswordAsync(user, this.Input.Token, this.Input.NewPassword);
            if (result.Succeeded)
            {
                this.PasswordReset = true;
            }
        }

        return this.Page();
    }
}
