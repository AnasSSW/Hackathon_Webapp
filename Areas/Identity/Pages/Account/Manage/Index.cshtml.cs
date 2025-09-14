// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable disable

using System.ComponentModel.DataAnnotations;
using Hackathon.Data; // Ensure this using is present
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json; // Required for JSON operations
using Hackathon.Models;
using Hackathon.Models;
namespace Hackathon.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        // Helper class for skills
        public class SkillViewModel
        {
            public string Category { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            public string ProfilePictureUrl { get; set; }

            [Display(Name = "New Profile Picture")]
            public IFormFile ProfilePictureFile { get; set; }

            [Display(Name = "ที่อยู่")]
            public string Address { get; set; }

            [Display(Name = "GitHub")]
            public string GitHubUsername { get; set; }

            [Display(Name = "วันเกิด")]
            [DataType(DataType.Date)]
            public DateOnly? DateOfBirth { get; set; }

            [Display(Name = "ภาษา")]
            public string Languages { get; set; }

            [Display(Name = "การศึกษา")]
            public string Education { get; set; }

            [Display(Name = "ประสบการณ์")]
            public string Experience { get; set; }

            // This will receive the JSON string from the form
            [Display(Name = "Skills")]
            public string SkillsJson { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = phoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Address = user.Address,
                GitHubUsername = user.GitHubUsername,
                DateOfBirth = user.DateOfBirth,
                Languages = user.Languages,
                Education = user.Education,
                Experience = user.Experience,
                SkillsJson = user.SkillsJson ?? "[]" // Ensure it's a valid JSON array string
            };
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

        public async Task<IActionResult> OnPostAsync()
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

            // --- Update all properties ---
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.Address = Input.Address;
            user.GitHubUsername = Input.GitHubUsername;
            user.DateOfBirth = Input.DateOfBirth;
            user.Languages = Input.Languages;
            user.Education = Input.Education;
            user.Experience = Input.Experience;
            user.SkillsJson = Input.SkillsJson; // Save the new JSON string

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (Input.ProfilePictureFile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Input.ProfilePictureFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                Directory.CreateDirectory(uploadsFolder);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.ProfilePictureFile.CopyToAsync(fileStream);
                }
                user.ProfilePictureUrl = "/uploads/" + uniqueFileName;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to update profile.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}

