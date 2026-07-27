using FormationManagement.Domain.Enums;
using FormationManagement.Infrastructure.Identity;
using FormationManagement.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

namespace FormationManagement.Web.Controllers;

/// <summary>
/// Hand-written Identity flow (rather than the scaffolded Razor Pages UI) so
/// authentication fits naturally into the MVC controller/Razor-view
/// architecture the rest of the app uses.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    // ---------------- Login ----------------

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in.", model.Email);
            return RedirectToLocal(model.ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account has been locked out due to multiple failed login attempts. Please try again later.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    // ---------------- Register ----------------

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Public self-registration always creates a Learner account.
            // Trainer/Administrator accounts are provisioned by an Administrator
            // from the Admin > Manage Users screen (see AdminUsersController).
            await _userManager.AddToRoleAsync(user, ApplicationRoles.Learner);

            _logger.LogInformation("New learner account created for {Email}.", model.Email);

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToLocal(returnUrl);
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    // ---------------- Logout ----------------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // ---------------- Forgot / Reset password ----------------

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);

        // Always show the same confirmation, whether or not the email exists,
        // to avoid leaking which addresses have an account.
        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            return RedirectToAction(nameof(ForgotPasswordConfirmation));

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = HtmlEncoder.Default.Encode(WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token)));

        var resetLink = Url.Action(nameof(ResetPassword), "Account",
            new { email = model.Email, token = encodedToken }, protocol: Request.Scheme);

        // NOTE: no email provider is wired up in this project (out of scope
        // per the brief — "Email Confirmation (optional)"). In development we
        // log the link so it can still be tested end-to-end; plug in
        // SendGrid/SMTP here for a real deployment.
        _logger.LogInformation("Password reset link for {Email}: {Link}", model.Email, resetLink);

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation() => View();

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? email = null, string? token = null)
    {
        if (email == null || token == null)
            return BadRequest("A valid email and reset token are required.");

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        return View(new ResetPasswordViewModel { Email = email, Token = decodedToken });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return RedirectToAction(nameof(ResetPasswordConfirmation)); // don't reveal whether the account exists

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

        if (result.Succeeded)
            return RedirectToAction(nameof(ResetPasswordConfirmation));

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation() => View();

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
}
