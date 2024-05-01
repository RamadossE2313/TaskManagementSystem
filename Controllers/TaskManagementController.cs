using Microsoft.AspNetCore.Mvc;

namespace TaskManagementSystem.Controllers
{
    public class TaskManagementController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
