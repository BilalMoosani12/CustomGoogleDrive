﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CustomGoogleDrive.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Googledriveapi.Pages.Account
{
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string LoginProvider { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login");
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToPage("./Login");
            }

            //var accessToken = info.AuthenticationTokens.Single(f => f.Name == "access_token").Value;
            //var tokenType = info.AuthenticationTokens.Single(f => f.Name == "token_type").Value;
            //var expiryDate = info.AuthenticationTokens.Single(f => f.Name == "expires_at").Value;
            await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {

                // Store the access token and resign in so the token is included in the cookie
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                var props = new AuthenticationProperties();
                props.StoreTokens(info.AuthenticationTokens);
                await _signInManager.SignInAsync(user, props, info.LoginProvider);

                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(Url.GetLocalUrl(returnUrl));
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                LoginProvider = info.LoginProvider;

                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var user = new ApplicationUser
                {
                    Email = email,
                    EmailConfirmed = true,
                    UserName = email
                };

                var userCreatedResult = await _userManager.CreateAsync(user);

                if (userCreatedResult.Succeeded)
                {
                    var loginResult = await _userManager.AddLoginAsync(user,
                        new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));

                    if (loginResult.Succeeded)
                    {
                        var props = new AuthenticationProperties();
                        props.StoreTokens(info.AuthenticationTokens);
                        await _signInManager.SignInAsync(user, props, info.LoginProvider);

                        await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                    }

                    else
                    {
                        foreach (var loginResultError in loginResult.Errors)
                        {
                            _logger.LogError($"Code: {loginResultError.Code}");
                            _logger.LogError($"Description: {loginResultError.Description}");
                        }
                    }
                }
                else
                {
                    foreach (var userCreatedError in userCreatedResult.Errors)
                    {
                        _logger.LogError($"Code: {userCreatedError.Code}");
                        _logger.LogError($"Description: {userCreatedError.Description}");
                    }
                }

                return LocalRedirect(Url.GetLocalUrl(returnUrl));

            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    throw new ApplicationException("Error loading external login information during confirmation.");
                }
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        // Copy over the gender claim as well
                        await _userManager.AddClaimAsync(user, info.Principal.FindFirst(ClaimTypes.Gender));

                        // Include the access token in the properties
                        var props = new AuthenticationProperties();
                        props.StoreTokens(info.AuthenticationTokens);

                        await _signInManager.SignInAsync(user, props, authenticationMethod: info.LoginProvider);
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        return LocalRedirect(Url.GetLocalUrl(returnUrl));
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ReturnUrl = returnUrl;
            return Page();
        }
    }
}