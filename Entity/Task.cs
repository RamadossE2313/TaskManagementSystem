namespace TaskManagementSystem.Entity
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Deadline { get; set; }
        public int StatusId { get; set; }
        public virtual Status Status { get; set; }
        public virtual ICollection<User> TeamMembers { get; set; } = new List<User>();
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
