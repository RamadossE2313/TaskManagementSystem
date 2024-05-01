namespace TaskManagementSystem.Entity
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }
    }
}
