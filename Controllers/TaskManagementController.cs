using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using TaskManagementSystem.Data;
using TaskManagementSystem.Entity;
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
            List<DashboardModel> dashboardList = new List<DashboardModel>();

            var tasks = _context.Tasks.Include(t => t.Status)
                                      .Include(t => t.Comments)
                                      .Include(t => t.TeamMembers).ToList();

            foreach (var task in tasks)
            {
                DashboardModel dashboardModel = new DashboardModel
                {
                    Id = task.Id,
                    Title = task.Title,
                    Deadline =  task.Deadline,
                    Status = task.Status.Name.ToString(),
                    AssignedUsers = string.Join(',', task.TeamMembers.Select(teamMember => teamMember.UserName).ToList()),
                    Comment = task.Comments?.FirstOrDefault()?.Text,
                };

                dashboardList.Add(dashboardModel);
            }
            return View(dashboardList);
        }

        public ActionResult AddTask()
        {
            var users = _context.Users;
            ViewData["Users"] = new SelectList(users, "Id", "UserName");

            var statusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "New" },
                new SelectListItem { Value = "2", Text = "In Progress" },
                new SelectListItem { Value = "3", Text = "Completed" }
            };
            ViewData["StatusOptions"] = statusOptions;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddTask(TaskModel taskModel)
        {
            if (ModelState.IsValid)
            {
                var task = new Entity.Task
                {
                    Title = taskModel.Title,
                    Deadline = taskModel.Deadline,
                    StatusId = taskModel.StatusId,
                };

                Comment addComment = await AddCommentAsync(taskModel);
                task.Comments.Add(addComment);

                if (taskModel.Files.Count() > 0)
                {
                    List<Attachment> attachments = await AddAttachmentAsync(taskModel);
                    task.Attachments.AddRange(attachments);
                }

                if(taskModel.AssignedUserIds.Count > 0)
                {
                    List<User> users = await AddAssignedUserAsync(taskModel);
                    task.TeamMembers.AddRange(users);
                }

                _context.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction("Dashboard");
            }

            return View(taskModel);
        }

        private async Task<Comment> AddCommentAsync(TaskModel taskModel)
        {
            var addComment = new Comment
            {
                Text = taskModel.Comment,
                PostedAt = DateTime.UtcNow,
            };

            _context.Comments.Add(addComment);
            await _context.SaveChangesAsync();
            return addComment;
        }

        private async Task<List<Attachment>> AddAttachmentAsync(TaskModel taskModel)
        {
            List<Attachment> attachments = new List<Attachment>();

            foreach (var file in taskModel.Files)
            {
                if (file.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var fileModel = new Attachment
                    {
                        FileName = uniqueFileName,
                        Length = file.Length,
                        ContentType = file.ContentType,
                        Data = System.IO.File.ReadAllBytes(filePath)
                    };

                    _context.Attachments.Add(fileModel);
                    await _context.SaveChangesAsync();
                    attachments.Add(fileModel);
                }
            }
            return attachments;
        }

        private async Task<List<User>> AddAssignedUserAsync(TaskModel taskModel)
        {
            List<User> assignedUsers = new List<User>();

            foreach (var userId in taskModel.AssignedUserIds)
            {
                var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == userId);
                if (user != null)
                {
                    assignedUsers.Add(user);
                }
            }

            return assignedUsers;
        }

        [HttpPost]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var taskEntity = await _context.Tasks.Include(st => st.Status)
                            .Include(cm => cm.Comments).FirstOrDefaultAsync(m => m.Id == id);


            TaskManagementSystem.Models.TaskModel taskModel = new TaskManagementSystem.Models.TaskModel
            {
                Id = taskEntity.Id,
                Title = taskEntity.Title,
                Deadline = taskEntity.Deadline,
                //Status = new List<StatusModel>
                //{
                //   new StatusModel{ Id=taskEntity.Id,Status=taskEntity.Status.Name}

                //}

            };


            if (taskModel == null)
            {
                return NotFound();
            }

            return View(taskModel);
        }
        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }



            var taskEntity = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);


            TaskModel taskModel = new TaskModel
            {
                Id = taskEntity.Id,
                Title = taskEntity.Title,
                Deadline = taskEntity.Deadline,
                StatusId = taskEntity.StatusId,

            };
            var statusOptions = _context.Statuses.ToList();
            ViewData["StatusOptions"] = new SelectList(statusOptions, "Id", "Name", taskEntity.StatusId);


            if (taskModel == null)
            {
                return NotFound();
            }

            return View(taskModel);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var task = _context.Tasks.Find(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }
    }
}
