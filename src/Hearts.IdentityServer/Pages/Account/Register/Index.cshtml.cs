using System.ComponentModel.DataAnnotations;
using Hearts.IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hearts.IdentityServer.Pages.Account.Register;

[AllowAnonymous]
public class IndexModel(UserManager<ApplicationUser> userManager,
    IEmailSender<ApplicationUser> emailSender) : PageModel
{
    [BindProperty]
    public RegisterInputModel Input { get; set; } = default!;

    public ViewModel View { get; set; } = default!;

    public class RegisterInputModel
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public IActionResult OnGet(string? returnUrl)
    {
        this.Input = new()
        {
            ReturnUrl = returnUrl
        };

        return this.Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (this.ModelState.IsValid)
        {
            ApplicationUser user = new()
            {
                UserName = this.Input.Username,
                Email = this.Input.Email
            };
            IdentityResult result = await userManager.CreateAsync(user, this.Input.Password!);

            if (result.Succeeded)
            {
                string token = await userManager.GenerateEmailConfirmationTokenAsync(user);                
                string confirmationLink = $"{this.Request.Scheme}://{this.Request.Host}{this.Url.Page("/Account/ConfirmEmail/Index", new { userId = user.Id, token })}";
                await emailSender.SendConfirmationLinkAsync(user, user.Email!, confirmationLink!);
                
                return this.RedirectToPage("/Account/Login/Index", this.Input.ReturnUrl);
            }

            foreach (IdentityError error in result.Errors)
            {
                this.ModelState.AddModelError("", error.Description);
            }
        }

        return this.Page();
    }
}
