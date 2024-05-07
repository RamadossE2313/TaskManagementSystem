namespace TaskManagementSystem.Entity
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime PostedAt { get; set; }

        public int TaskId { get; set; }
        public virtual Task Task { get; set; }
    }
}
