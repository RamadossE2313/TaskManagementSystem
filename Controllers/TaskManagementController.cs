using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Controllers
{
    public class TaskManagementController : Controller
    {
        private readonly TaskManagementDBContext _context;
        public TaskManagementController(TaskManagementDBContext taskManagementDBContext)
        {
            _context = taskManagementDBContext;
        }

        /// <summary>
        /// To display all the task details
        /// </summary>
        /// <returns></returns>
        public IActionResult Dashboard(int departmentId)
        {
            var tasks = _context.Tasks;
            var taskListByDepartments = tasks.Where(t => t.TeamMembers.Where(x => x.DepartmentId == departmentId).Count() > 0).ToList();
            return View(taskListByDepartments);
        }

        public ActionResult Add()
        {
            var users = _context.Users;
            ViewData["Users"] = new SelectList(users, "Id", "UserName");

            var statusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "New" },
                new SelectListItem { Value = "1", Text = "In Progress" },
                new SelectListItem { Value = "2", Text = "Completed" }
            };
            ViewData["StatusOptions"] = statusOptions;

            return View();
        }

        [HttpPost]
        public ActionResult Add(TaskModel taskModel)
        {
            if (ModelState.IsValid)
            {
                _context.Tasks.Add(new Entity.Task());
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            }

            return View(taskModel);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var task = _context.Tasks.Find(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
