using Hearts.IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hearts.IdentityServer.Pages.Account.ResetPasswordLink;

[SecurityHeaders]
[AllowAnonymous]
public class IndexModel(UserManager<ApplicationUser> userManager,
    IEmailSender<ApplicationUser> emailSender) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public bool LinkSent { get; set; } = false;

    public async Task<IActionResult> OnPostAsync()
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(this.Input.Email);
        if (user == null)
        {
            this.ModelState.AddModelError("", "User not found");
            return this.Page();
        }

        string token = await userManager.GeneratePasswordResetTokenAsync(user);
        string resetLink = $"{this.Request.Scheme}://{this.Request.Host}{this.Url.Page("/Account/ResetPassword/Index", new { this.Input.Email, token })}";
        await emailSender.SendPasswordResetLinkAsync(user, user.Email!, resetLink);

        this.LinkSent = true;

        return this.Page();
    }
}
