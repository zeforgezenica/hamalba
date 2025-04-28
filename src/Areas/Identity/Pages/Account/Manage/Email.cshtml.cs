// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using hamalba.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace hamalba.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly SignInManager<Korisnik> _signInManager;
        private readonly IEmailSender _emailSender;

        public EmailModel(
            UserManager<Korisnik> userManager,
            SignInManager<Korisnik> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool IsEmailConfirmed { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(Korisnik user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail != email)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, email = Input.NewEmail, code = code },
                    protocol: Request.Scheme);
                await _emailSender.SendEmailAsync(
                    Input.NewEmail,
                    "Verifikacija računa",
                     $@"
                                <div style='font-family: Arial, sans-serif; color: #333; padding: 20px;'>
                                    <h2 style='color: #2c3e50;'>Welcome to Our App!</h2>
                                    <p>Thanks for signing up. Please confirm your account by clicking the button below:</p>
                                    <p style='text-align: center; margin: 30px 0;'>
                                        <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'
                                           style='background-color: #4CAF50; color: white; padding: 12px 20px;
                                                  text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                                            Confirm Email
                                        </a>
                                    </p>
                                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                                    <p style='font-size: 0.9em; color: #555;'>{HtmlEncoder.Default.Encode(callbackUrl)}</p>
                                    <hr style='margin-top: 40px;'>
                                    <p style='font-size: 0.8em; color: #999;'>If you didn’t request this, you can safely ignore this email.</p>
                                </div>
                            ");

                StatusMessage = "Confirmation link to change email sent. Please check your email.";
                return RedirectToPage();
            }

            StatusMessage = "Your email is unchanged.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                email,
                "Verifikacija računa",
                 $@"
                                <div style='font-family: Arial, sans-serif; color: #333; padding: 20px;'>
                                    <h2 style='color: #2c3e50;'>Dobrodošli na HamalBa!</h2>
                                    <p>Hvala što ste se registrovali. Molimo vas da potvrdite svoj račun klikom na dugme ispod:</p>
                                    <p style='text-align: center; margin: 30px 0;'>
                                        <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'
                                           style='background-color: #4CAF50; color: white; padding: 12px 20px;
                                                  text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                                            Verifikuj se
                                        </a>
                                    </p>
                                    
                                </div>
                            ");

            StatusMessage = "Verification email sent. Please check your email.";
            return RedirectToPage();
        }
    }
}
