using Garden_group.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Garden_group.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IActionResult Index()
        {
            List<User> users = _userRepository.GetAllUser();

            return View(users);
        }

        public IActionResult Details(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId userId))
            {
                return BadRequest();
            }

            User? user = _userRepository.GetById(userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new User());
        }

        [HttpPost]
        public IActionResult Create(User user)
        {
            if (!IsValid(user, true))
            {
                return View(user);
            }

            user.Id = ObjectId.GenerateNewId();
            _userRepository.Create(user);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId userId))
            {
                return BadRequest();
            }

            User? user = _userRepository.GetById(userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(string id, User user)
        {
            if (!ObjectId.TryParse(id, out ObjectId userId))
            {
                return BadRequest();
            }

            User? existingUser = _userRepository.GetById(userId);

            if (existingUser == null)
            {
                return NotFound();
            }

            user.Id = userId;

            if (string.IsNullOrWhiteSpace(user.Password))
            {
                user.Password = existingUser.Password;
            }

            if (!IsValid(user, false))
            {
                return View(user);
            }

            _userRepository.Update(userId, user);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId userId))
            {
                return BadRequest();
            }

            bool deleted = _userRepository.Delete(userId);

            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }

        private bool IsValid(User user, bool isNewUser)
        {
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(user.Name.First) || string.IsNullOrWhiteSpace(user.Name.Last))
            {
                ModelState.AddModelError("", "First name and last name are required.");
            }

            if (string.IsNullOrWhiteSpace(user.Email) || !user.Email.Contains("@"))
            {
                ModelState.AddModelError("", "Please enter a valid e-mail address.");
            }

            if (isNewUser && string.IsNullOrWhiteSpace(user.Password))
            {
                ModelState.AddModelError("", "A password is required for a new user.");
            }

            user.Phone = user.Phone ?? "";
            user.Address = user.Address ?? "";

            return ModelState.ErrorCount == 0;
        }
    }
}
