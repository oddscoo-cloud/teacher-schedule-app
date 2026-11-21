
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
        public DbSet<StudentCourseAssignmentEntity> StudentCourseAssignments { get; set; }
        public DbSet<ContactEntity> Contacts { get; set; }
        public DbSet<StudentContactEntity> StudentContacts { get; set; }
        public DbSet<CourseEntity> Courses { get; set; }
        public DbSet<StudentNoteEntity> StudentNotes { get; set; }
        public DbSet<StudentPriceHistoryEntity> StudentPriceHistory { get; set; }
        public DbSet<StudentPaymentEntity> StudentPayments { get; set; }
        public DbSet<StudentPaymentAdjustmentEntity> StudentPaymentAdjustments { get; set; }
        public DbSet<StudentLessonChargeEntity> StudentLessonCharges { get; set; }
        public DbSet<HomeworkEntity> Homeworks { get; set; }
        public DbSet<HomeworkSubmissionEntity> HomeworkSubmissions { get; set; }
        public DbSet<HomeworkAttachmentEntity> HomeworkAttachments { get; set; }
        public DbSet<LessonSeriesEntity> LessonSeries { get; set; }
        public DbSet<LessonEntity> LessonEvents { get; set; }
        public DbSet<LessonRescheduleEntity> LessonReschedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
            ConfigureStudentFinanceEntities(modelBuilder);
            ConfigureHomeworkEntities(modelBuilder);
            ConfigureLessonEntities(modelBuilder);
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

        private void ConfigureStudentFinanceEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudentPriceHistoryEntity>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.Amount).HasPrecision(18, 2);
                entity.HasOne(item => item.Student)
                    .WithMany(student => student.PriceHistory)
                    .HasForeignKey(item => item.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.RecordedByUser)
                    .WithMany()
                    .HasForeignKey(item => item.RecordedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(item => new { item.StudentId, item.EffectiveFrom }).IsUnique();
            });

            modelBuilder.Entity<StudentPaymentEntity>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.Amount).HasPrecision(18, 2);
                entity.Property(item => item.IdempotencyKey).IsRequired().HasMaxLength(100);
                entity.HasOne(item => item.Student)
                    .WithMany(student => student.Payments)
                    .HasForeignKey(item => item.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.RecordedByUser)
                    .WithMany()
                    .HasForeignKey(item => item.RecordedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(item => new { item.StudentId, item.IdempotencyKey }).IsUnique();
            });

            modelBuilder.Entity<StudentPaymentAdjustmentEntity>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.AmountDelta).HasPrecision(18, 2);
                entity.Property(item => item.Reason).IsRequired().HasMaxLength(500);
                entity.Property(item => item.IdempotencyKey).IsRequired().HasMaxLength(100);
                entity.HasOne(item => item.Payment)
                    .WithMany(payment => payment.Adjustments)
                    .HasForeignKey(item => item.PaymentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.RecordedByUser)
                    .WithMany()
                    .HasForeignKey(item => item.RecordedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(item => new { item.PaymentId, item.IdempotencyKey }).IsUnique();
            });

            modelBuilder.Entity<StudentLessonChargeEntity>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.Amount).HasPrecision(18, 2);
                entity.HasOne(item => item.Student)
                    .WithMany(student => student.LessonCharges)
                    .HasForeignKey(item => item.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.RecordedByUser)
                    .WithMany()
                    .HasForeignKey(item => item.RecordedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(item => new { item.StudentId, item.SourceLessonId }).IsUnique();
            });
        }

        private void ConfigureHomeworkEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HomeworkEntity>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.Title).IsRequired().HasMaxLength(255);
                entity.Property(item => item.Instructions).HasMaxLength(4000);
                entity.HasOne(item => item.Student)
                    .WithMany()
                    .HasForeignKey(item => item.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Teacher)
                    .WithMany()
                    .HasForeignKey(item => item.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.School)
                    .WithMany()
                    .HasForeignKey(item => item.SchoolId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(item => item.Course)
                    .WithMany()
                    .HasForeignKey(item => item.CourseId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(item => new { item.StudentId, item.CreatedAt });
                entity.HasIndex(item => new { item.TeacherId, item.CourseId });
                entity.HasIndex(item => item.SchoolId);
            });

            modelBuilder.Entity<HomeworkSubmissionEntity>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.LinksJson).IsRequired();
                entity.HasOne(item => item.Homework)
                    .WithMany(homework => homework.Submissions)
                    .HasForeignKey(item => item.HomeworkId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(item => new { item.HomeworkId, item.AttemptNumber }).IsUnique();
            });

            modelBuilder.Entity<HomeworkAttachmentEntity>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.StorageName).IsRequired().HasMaxLength(80);
                entity.Property(item => item.DisplayName).IsRequired().HasMaxLength(255);
                entity.Property(item => item.ContentType).IsRequired().HasMaxLength(100);
                entity.HasOne(item => item.Submission)
                    .WithMany(submission => submission.Attachments)
                    .HasForeignKey(item => item.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(item => item.SubmissionId);
            });
        }

        private void ConfigureLessonEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LessonSeriesEntity>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.Title).IsRequired().HasMaxLength(255);
                entity.Property(item => item.Description).HasMaxLength(4000);
                entity.Property(item => item.Timezone).IsRequired().HasMaxLength(100);
                entity.HasOne<TeacherEntity>().WithMany().HasForeignKey(item => item.TeacherId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<StudentEntity>().WithMany().HasForeignKey(item => item.StudentId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<SchoolEntity>().WithMany().HasForeignKey(item => item.SchoolId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<CourseEntity>().WithMany().HasForeignKey(item => item.CourseId).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(item => new { item.TeacherId, item.StudentId, item.StartDate });
            });

            modelBuilder.Entity<LessonEntity>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.Title).IsRequired().HasMaxLength(255);
                entity.Property(item => item.Description).HasMaxLength(4000);
                entity.Property(item => item.Notes).HasMaxLength(4000);
                entity.Property(item => item.StatusReason).HasMaxLength(500);
                entity.Property(item => item.Timezone).IsRequired().HasMaxLength(100);
                entity.HasOne(item => item.Series).WithMany(series => series.Lessons)
                    .HasForeignKey(item => item.SeriesId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<TeacherEntity>().WithMany().HasForeignKey(item => item.TeacherId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Student).WithMany().HasForeignKey(item => item.StudentId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<SchoolEntity>().WithMany().HasForeignKey(item => item.SchoolId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Course).WithMany().HasForeignKey(item => item.CourseId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<UserEntity>().WithMany().HasForeignKey(item => item.ModifiedByUserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.RescheduledFrom).WithMany(item => item.RescheduledTo)
                    .HasForeignKey(item => item.RescheduledFromId).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(item => new { item.TeacherId, item.StudentId, item.ScheduledAtUtc });
                entity.HasIndex(item => new { item.SeriesId, item.Status });
                entity.HasIndex(item => new { item.SeriesId, item.SeriesInstanceIndex })
                    .IsUnique().HasFilter("[SeriesId] IS NOT NULL AND [SeriesInstanceIndex] IS NOT NULL");
            });

            modelBuilder.Entity<LessonRescheduleEntity>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.RescheduleReason).HasMaxLength(500);
                entity.HasOne(item => item.OriginalLesson).WithMany(item => item.RescheduleHistoryAsOriginal)
                    .HasForeignKey(item => item.OriginalLessonId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.NewLesson).WithMany(item => item.RescheduleHistoryAsNew)
                    .HasForeignKey(item => item.NewLessonId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<UserEntity>().WithMany().HasForeignKey(item => item.RescheduleByUserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(item => item.OriginalLessonId).IsUnique();
                entity.HasIndex(item => item.NewLessonId).IsUnique();
            });
        }
    }
}
