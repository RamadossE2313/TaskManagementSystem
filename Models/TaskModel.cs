using System.ComponentModel.DataAnnotations;

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
        public virtual ICollection<UserModel> TeamMembers { get; set; }
        public List<CommentsModel> Comments { get; set; }
        public List<AttachmentModel> Attachments { get; set; }
    }
}
