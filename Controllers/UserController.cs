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
    }
}
