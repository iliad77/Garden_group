
using Garden_group.Models;
using Garden_group.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;


namespace Garden_group.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher<User> _passwordHasher;

        public AccountController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = new PasswordHasher<User>();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string email = model.Email.Trim().ToLowerInvariant();

            // Check whether an account already exists
            User? existingUser = _userRepository.GetByEmail(email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "An account with this email already exists."
                );

                return View(model);
            }

            // Create a new regular employee
            User user = new User
            {
                Id = ObjectId.GenerateNewId(),

                Name = new Name
                {
                    First = model.FirstName.Trim(),
                    Last = model.LastName.Trim()
                },

                Email = email,
                Phone = "",
                Address = "",
                UserType = UserType.Employee
            };

            // Hash the password before saving
            user.Password = _passwordHasher.HashPassword(
                user,
                model.Password
            );

            // Save the new user to MongoDB
            _userRepository.Create(user);

            TempData["SuccessMessage"] =
                "Your account was created successfully.";

            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string email = model.Email.Trim().ToLowerInvariant();

            // Find the user in MongoDB
            User? user = _userRepository.GetByEmail(email);

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password."
                );

                return View(model);
            }

            // Verify the hashed password
            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.Password,
                model.Password
            );

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password."
                );

                return View(model);
            }

            // Create claims for the authenticated user
            var claims = new List<Claim>
    {
        new Claim(
            ClaimTypes.NameIdentifier,
            user.Id.ToString()
        ),

        new Claim(
            ClaimTypes.Name,
            $"{user.Name.First} {user.Name.Last}"
        ),

        new Claim(
            ClaimTypes.Email,
            user.Email
        ),

        new Claim(
            ClaimTypes.Role,
            user.UserType.ToString()
        )
    };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            // Sign in and create the authentication cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return RedirectToAction("Index", "Dashboard");
        }

    }
}

