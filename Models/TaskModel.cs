﻿using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.Models
{
    public class TaskModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }
        
        [Required]
        public DateTime Deadline { get; set; }
        public int DepartmentId { get; set; }
        public int StatusId { get; set; }
        public List<int> AssignedUserIds { get; set; }
        public string Comment { get; set; }
        public IEnumerable<IFormFile> Files { get; set; } = new List<IFormFile>();
    }
}
