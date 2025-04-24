using Hearts.IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hearts.IdentityServer.Pages.Account.ConfirmEmail;

[AllowAnonymous]
public class IndexModel(UserManager<ApplicationUser> userManager) : PageModel
{
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            this.StatusMessage = "Invalid confirmation request.";
            return this.Page();
        }

        ApplicationUser? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            this.StatusMessage = "User not found.";
            return this.Page();
        }        

        IdentityResult result = await userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            this.StatusMessage = "Thank you for confirming your email.";
        }
        else
        {
            this.StatusMessage = "Error confirming your email.";
        }

        return this.Page();
    }
}
