namespace TaskManagementSystem.Entity
{
    public class Status
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int TaskId { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }
    }
}
