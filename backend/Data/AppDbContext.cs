
using backend.Core.Entities;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        // Legacy tables
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<User> Users { get; set; }

        // New Clean Architecture tables
        public DbSet<UserEntity> UserEntities { get; set; }
        public DbSet<SchoolEntity> Schools { get; set; }
        public DbSet<WorkspaceMembershipEntity> WorkspaceMemberships { get; set; }
        public DbSet<TeacherEntity> TeacherEntities { get; set; }
        public DbSet<StudentEntity> StudentEntities { get; set; }
        public DbSet<StudentAssignmentEntity> StudentAssignments { get; set; }
        public DbSet<ContactEntity> Contacts { get; set; }
        public DbSet<StudentContactEntity> StudentContacts { get; set; }
        public DbSet<CourseEntity> Courses { get; set; }
        public DbSet<StudentNoteEntity> StudentNotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Legacy data - marked for removal after migration
            modelBuilder.Entity<Teacher>().HasData(
                new Teacher { Id = 1, Name = "John Doe" },
                new Teacher { Id = 2, Name = "Jane Smith" }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Login = "admin",
                    Password = "admin",
                    Role = "admin",
                    TeacherId = null
                },
                new User
                {
                    Id = 2,
                    Login = "teacher1",
                    Password = "123",
                    Role = "teacher",
                    TeacherId = 1
                },
                new User
                {
                    Id = 3,
                    Login = "teacher2",
                    Password = "1234",
                    Role = "teacher",
                    TeacherId = 2
                }
            );

            // Configure new Clean Architecture entities
            ConfigureUserEntity(modelBuilder);
            ConfigureSchoolEntity(modelBuilder);
            ConfigureWorkspaceMembershipEntity(modelBuilder);
            ConfigureTeacherEntity(modelBuilder);
            ConfigureStudentEntity(modelBuilder);
            ConfigureStudentAssignmentEntity(modelBuilder);
            ConfigureContactEntities(modelBuilder);
            ConfigureCourseEntity(modelBuilder);
            ConfigureStudentNoteEntity(modelBuilder);
        }

        private void ConfigureUserEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasMany(e => e.WorkspaceMemberships)
                    .WithOne(w => w.User)
                    .HasForeignKey(w => w.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureSchoolEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SchoolEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.HasMany(e => e.Memberships)
                    .WithOne(w => w.School)
                    .HasForeignKey(w => w.SchoolId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasMany(e => e.Students)
                    .WithOne(s => s.School)
                    .HasForeignKey(s => s.SchoolId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        private void ConfigureWorkspaceMembershipEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkspaceMembershipEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WorkspaceType).IsRequired();
                entity.HasOne(e => e.User)
                    .WithMany(u => u.WorkspaceMemberships)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.School)
                    .WithMany(s => s.Memberships)
                    .HasForeignKey(e => e.SchoolId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        private void ConfigureTeacherEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TeacherEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(255);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.School)
                    .WithMany()
                    .HasForeignKey(e => e.SchoolId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasMany(e => e.StudentAssignments)
                    .WithOne(sa => sa.Teacher)
                    .HasForeignKey(sa => sa.TeacherId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.StandaloneStudents)
                    .WithOne(s => s.StandaloneTeacher)
                    .HasForeignKey(s => s.StandaloneTeacherId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        private void ConfigureStudentEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudentEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Patronymic).HasMaxLength(255);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.School)
                    .WithMany(s => s.Students)
                    .HasForeignKey(e => e.SchoolId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.StandaloneTeacher)
                    .WithMany(t => t.StandaloneStudents)
                    .HasForeignKey(e => e.StandaloneTeacherId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasMany(e => e.TeacherAssignments)
                    .WithOne(sa => sa.Student)
                    .HasForeignKey(sa => sa.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.SchoolId);
            });
        }

        private void ConfigureStudentAssignmentEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudentAssignmentEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("StudentTeacherAssignments");
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.TeacherAssignments)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Teacher)
                    .WithMany(t => t.StudentAssignments)
                    .HasForeignKey(e => e.TeacherId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.IsSchool).HasDefaultValue(false);
                entity.HasIndex(e => new { e.StudentId, e.TeacherId }).IsUnique();
                entity.HasIndex(e => e.TeacherId);
            });
        }

        private void ConfigureContactEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContactEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            });

            modelBuilder.Entity<StudentContactEntity>(entity =>
            {
                entity.HasKey(e => new { e.StudentId, e.ContactId });
                entity.Property(e => e.Relationship).HasMaxLength(100);
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.Contacts)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Contact)
                    .WithMany(c => c.Students)
                    .HasForeignKey(e => e.ContactId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureCourseEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CourseEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.HasOne(e => e.School)
                    .WithMany()
                    .HasForeignKey(e => e.SchoolId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.SchoolId, e.Name }).IsUnique();
            });

            modelBuilder.Entity<StudentCourseAssignmentEntity>(entity =>
            {
                entity.HasKey(e => new { e.StudentId, e.CourseId });
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.CourseAssignments)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Course)
                    .WithMany(c => c.Students)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.CourseId);
            });
        }

        private void ConfigureStudentNoteEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudentNoteEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.Notes)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Teacher)
                    .WithMany()
                    .HasForeignKey(e => e.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => new { e.StudentId, e.TeacherId });
            });
        }
    }
}
