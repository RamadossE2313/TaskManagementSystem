using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskManagementSystem.Data;

namespace TaskManagementSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly TaskManagementDBContext _context;


        public LoginController(TaskManagementDBContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == username && u.Password == password);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, "User"), // Assuming some role
                    new Claim(ClaimTypes.Authentication, "true") // Claim indicating authentication status
                };
                var identity = new ClaimsIdentity(claims, "Custom");

                // Configure JSON serialization options to handle object cycles
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve
                };

                // Serialize the identity to a JSON string
                var identityString = JsonSerializer.Serialize(identity, options);

                // Store the JSON string in the session
                HttpContext.Session.SetString("Identity", identityString);


                ViewData["isAuth"] = true;
                ViewData["departmentId"] = user.DepartmentId;
                //return View();
                return RedirectToAction("Dashboard", "TaskManagement", new { departmentId = user.DepartmentId });
            }
            else
            {
                ModelState.AddModelError("Invalid username or password", "Invalid username or password");
                return View();
            }
        }
    }
}
