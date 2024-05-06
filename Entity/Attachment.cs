namespace TaskManagementSystem.Entity
{
    public class Attachment
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public long Length { get; set; }
        public string ContentType { get; set; }
        public byte[] Data { get; set; }
    }
}
