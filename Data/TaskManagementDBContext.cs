using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Entity;

namespace TaskManagementSystem.Data
{
    public class TaskManagementDBContext : DbContext
    {
        public TaskManagementDBContext(DbContextOptions<TaskManagementDBContext> options) : 
            base(options)
        {
                
        }
        public DbSet<Entity.Task> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "IT" },
                new Department { Id = 2, Name = "HR" },
                new Department { Id = 3, Name = "NON IT" },
                new Department { Id = 4, Name = "Admin" }
                );

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, UserName = "TestUser1", Password = "TestUser1", DepartmentId = 1 },
                new User { Id = 2, UserName = "TestUser2", Password = "TestUser2", DepartmentId = 2 },
                new User { Id = 3, UserName = "TestUser3", Password = "TestUser3", DepartmentId = 3 },
                new User { Id = 4, UserName = "Master", Password = "Master", DepartmentId = 4 }
            );

            modelBuilder.Entity<Status>().HasData(
                new Status { Id = 1, Name = "New"},
                new Status { Id = 2, Name = "In Progress"},
                new Status { Id = 3, Name = "Completed"}
            );
        }

    }
}
