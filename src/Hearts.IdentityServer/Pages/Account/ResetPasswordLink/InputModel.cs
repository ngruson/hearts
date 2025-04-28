using System.ComponentModel.DataAnnotations;

namespace Hearts.IdentityServer.Pages.Account.ResetPasswordLink;

public class InputModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;
}
