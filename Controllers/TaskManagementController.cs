﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using TaskManagementSystem.Data;
using TaskManagementSystem.Entity;
using TaskManagementSystem.Models;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaskManagementSystem.Controllers
{
    public class TaskManagementController : Controller
    {
        private readonly TaskManagementDBContext _context;
        private readonly ILogger<TaskManagementController> _logger;
        public TaskManagementController(TaskManagementDBContext taskManagementDBContext, ILogger<TaskManagementController> logger)
        {
            _context = taskManagementDBContext;
            _logger = logger;
        }

        /// <summary>
        /// To display all the task details
        /// </summary>
        /// <returns></returns>
        public IActionResult Dashboard(int departmentId)
        {
            List<DashboardModel> dashboardList = new List<DashboardModel>();
            try
            {
                if (!isAuth())
                {
                    return Redirect("~/");
                }

                var tasks = _context.Tasks.Include(t => t.Status)
                                     .Include(t => t.Comments)
                                     .Include(t => t.TeamMembers)
                                     .Include(t => t.Attachments)
                                     //.Where(t => t.TeamMembers.Any(tm => tm.DepartmentId == departmentId))
                                     .ToList();

                foreach (var task in tasks)
                {
                    DashboardModel dashboardModel = new DashboardModel
                    {
                        Id = task.Id,
                        Title = task.Title,
                        Deadline = task.Deadline,
                        Status = task.Status.Name.ToString(),
                        AssignedUsers = string.Join(',', task.TeamMembers.Select(teamMember => teamMember.UserName).ToList()),
                        Comment = task.Comments?.OrderByDescending(c => c.PostedAt)?.FirstOrDefault()?.Text,
                        Attachment = task.Attachments?.FirstOrDefault()?.FileName,
                    };

                    dashboardList.Add(dashboardModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving dashboard.");
                return View("Error");
            }
            return View(dashboardList);
        }

       

        public IActionResult Kanban(int departmentId)
        {
            List<DashboardModel> dashboardList = new List<DashboardModel>();
            try
            {
                if (!isAuth())
                {
                    return Redirect("~/");
                }



                var tasks = _context.Tasks.Include(t => t.Status)
                                          .Include(t => t.Comments)
                                          .Include(t => t.Attachments)
                                          .Include(t => t.TeamMembers)
                                          //.Where(t => t.TeamMembers.Any(tm => tm.DepartmentId == departmentId))
                                          .ToList();

                foreach (var task in tasks)
                {
                    DashboardModel dashboardModel = new DashboardModel
                    {
                        Id = task.Id,
                        Title = task.Title,
                        Deadline = task.Deadline,
                        Status = task.Status.Name.ToString(),
                        AssignedUsers = string.Join(',', task.TeamMembers.Select(teamMember => teamMember.UserName).ToList()),
                        Comment = task.Comments?.OrderByDescending(c => c.PostedAt)?.FirstOrDefault()?.Text,
                        Attachment = task.Attachments?.FirstOrDefault()?.FileName,
                    };

                    dashboardList.Add(dashboardModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving Kanban.");
                return View("Error");
            }

            return View(dashboardList);
        }

        public ActionResult AddTask()
        {
            try
            {
                var users = _context.Users;
                ViewData["Users"] = new SelectList(users, "Id", "UserName");

                var statusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "New" },
                new SelectListItem { Value = "2", Text = "Pending" },
                new SelectListItem { Value = "3", Text = "Done" }
            };
                ViewData["StatusOptions"] = statusOptions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving dashboard data.");
                return View("Error");
            }

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddTask(TaskModel taskModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var task = new Entity.Task
                    {
                        Title = taskModel.Title,
                        Deadline = taskModel.Deadline,
                        StatusId = taskModel.StatusId,
                    };

                    if (taskModel.AssignedUserIds.Count > 0)
                    {
                        List<User> users = await AddAssignedUserAsync(taskModel);
                        task.TeamMembers.AddRange(users);
                    }

                    _context.Add(task);
                    await _context.SaveChangesAsync();

                    Comment addComment = await AddCommentAsync(taskModel, task.Id);
                    task.Comments.Add(addComment);

                    if (taskModel.Files.Count() > 0)
                    {
                        List<Attachment> attachments = await AddAttachmentAsync(taskModel, task.Id);
                        task.Attachments.AddRange(attachments);
                    }

                    return RedirectToAction("Dashboard", new { success = true });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving dashboard data.");
                return View("Error");
            }

            

            return View(taskModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskModel taskModel)
        {

            //var users = _context.Users;
            //ViewData["Users"] = new SelectList(users, "Id", "UserName");

            var users = await _context.Users.ToListAsync();
            //ViewData["Users"] = new SelectList(users, "Id", "UserName", taskModel.AssignedUserIds);

            if (taskModel.Title != null)
            {
                var taskEntity = await _context.Tasks
                                                    .Include(st => st.Status)
                                                    .Include(cm => cm.Comments)
                                                    .Include(at => at.Attachments)
                                                    .Include(t => t.TeamMembers)
                                                    .FirstOrDefaultAsync(t => t.Id == id);

                taskEntity.Title = taskModel.Title;
                taskEntity.Deadline = taskModel.Deadline;
                taskEntity.StatusId = taskModel.StatusId;

                Comment addComment = await UpdateCommentAsync(taskModel, id);


                _context.Tasks.Update(taskEntity);
                await _context.SaveChangesAsync();
                return RedirectToAction("Dashboard", new { success = true });
            }
            else
            {
                if (id == null)
                {
                    return NotFound();
                }
                var taskEntity = await _context.Tasks
                    .Include(st => st.Status)
                    .Include(cm => cm.Comments)
                    .Include(at => at.Attachments)
                    .Include(t=>t.TeamMembers)
                    .FirstOrDefaultAsync(t => t.Id == id);
                taskModel = new TaskModel
                {
                    Id = taskEntity.Id,
                    Title = taskEntity.Title,
                    Deadline = taskEntity.Deadline,
                    StatusId = taskEntity.StatusId,
                    Comment = taskEntity.Comments?.OrderByDescending(c => c.PostedAt)?.FirstOrDefault()?.Text,
                    
                };

                var selectedUserIds = taskEntity.TeamMembers.Select(u => u.Id).ToList();
                var userSelectList = users.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.UserName,
                    Selected = selectedUserIds.Contains(u.Id),
                    Group = new SelectListGroup { Name = selectedUserIds.Contains(u.Id) ? "Assigned" : "Not Assigned" } // Assign users to groups based on their assignment status
                }).ToList();
                ViewData["Users"] = new SelectList(userSelectList, "Value", "Text", selectedUserIds); // Pass selected IDs to preselect users


                var statusOptions = _context.Statuses.ToList();
                ViewData["StatusOptions"] = new SelectList(statusOptions, "Id", "Name", taskEntity.StatusId);
                return View(taskModel);
            }
        }


        private async Task<Comment> AddCommentAsync(TaskModel taskModel, int taskId)
        {
            try
            {
                var addComment = new Comment
                {
                    Text = taskModel.Comment,
                    PostedAt = DateTime.UtcNow,
                    TaskId = taskId
                };

                _context.Comments.Add(addComment);
                await _context.SaveChangesAsync();
                return addComment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving dashboard data.");
                return new Comment();
            }

        }

        private async Task<Comment> UpdateCommentAsync(TaskModel taskModel, int taskId)
        {
            try
            {
                var addComment = new Comment
                {
                    Text = taskModel.Comment,
                    PostedAt = DateTime.UtcNow,
                    TaskId = taskId
                };

                _context.Comments.Update(addComment);
                await _context.SaveChangesAsync();
                return addComment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving dashboard data.");
                return new Comment();
            }

        }

        private async Task<List<Attachment>> AddAttachmentAsync(TaskModel taskModel, int taskId)
        {
            List<Attachment> attachments = new List<Attachment>();

            try
            {

                foreach (var file in taskModel.Files)
                {
                    if (file.Length > 0)
                    {
                        var uniqueFileName = file.FileName + Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

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
                            Data = System.IO.File.ReadAllBytes(filePath),
                            TaskId = taskId
                        };

                        _context.Attachments.Add(fileModel);
                        await _context.SaveChangesAsync();
                        attachments.Add(fileModel);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving dashboard data.");
            }

            return attachments;
        }

        private async Task<List<User>> AddAssignedUserAsync(TaskModel taskModel)
        {
            List<User> assignedUsers = new List<User>();

            try
            {
                foreach (var userId in taskModel.AssignedUserIds)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == userId);
                    if (user != null)
                    {
                        assignedUsers.Add(user);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving dashboard data.");

            }

            return assignedUsers;
        }

        [HttpPost]
        public async Task<IActionResult> Details(int? id)
        {
            TaskManagementSystem.Models.TaskModel taskModel;

            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var taskEntity = await _context.Tasks.Include(st => st.Status)
                          .Include(cm => cm.Comments).FirstOrDefaultAsync(m => m.Id == id);
                taskModel = new TaskManagementSystem.Models.TaskModel
                {
                    Id = taskEntity.Id,
                    Title = taskEntity.Title,
                    Deadline = taskEntity.Deadline,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving dashboard data.");
                return View("Error");
            }
            if (taskModel == null)
            {
                return NotFound();
            }

            return View(taskModel);
        }

       
        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }


        //public async Task<IActionResult> Edit(int? id)
        //{
        //    TaskModel taskModel;

        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    try
        //    {
        //        var taskEntity = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        //        taskModel = new TaskModel
        //        {
        //            Id = taskEntity.Id,
        //            Title = taskEntity.Title,
        //            Deadline = taskEntity.Deadline,
        //            StatusId = taskEntity.StatusId,

        //        };
        //         var statusOptions = _context.Statuses.ToList();
        //        ViewData["StatusOptions"] = new SelectList(statusOptions, "Id", "Name", taskEntity.StatusId);

        //        //var taskModEnt = _context.Update(taskEntity);
        //        //var Testuser1 = await _context.SaveChangesAsync();
        //        //return RedirectToAction("Dashboard", new { success = true });

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An error occurred while retrieving dashboard data.");
        //        return View("Error");
        //    }
        //    if (taskModel == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(taskModel);
        //}

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var task = _context.Tasks.Find(id);
                if (task == null)
                {
                    return NotFound();
                }

                _context.Tasks.Remove(task);
                _context.SaveChanges();
                return RedirectToAction("Dashboard", new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving dashboard data.");
                return View("Error");
            }
        }

        public bool isAuth()
        {
            bool isAuthenticated = false;
            try
            {
                var identityString = HttpContext.Session.GetString("Identity");
                if (identityString != null)
                {
                    //var identity = JsonSerializer.Deserialize<ClaimsIdentity>(identityString);
                    using (JsonDocument document = JsonDocument.Parse(identityString))
                    {
                        JsonElement root = document.RootElement;
                        JsonElement isAuthenticatedElement = root.GetProperty("IsAuthenticated");
                        isAuthenticated = isAuthenticatedElement.GetBoolean();
                    }

                }
                else
                {

                    return false;
                }
                if (!isAuthenticated)
                {
                    return false;
                }
                ViewData["isAuth"] = isAuthenticated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving dashboard data.");
                return false;
            }

            return isAuthenticated;
        }

    }
}
