using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourseAssignmentEntity_Courses_CourseId",
                table: "StudentCourseAssignmentEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourseAssignmentEntity_StudentEntities_StudentId",
                table: "StudentCourseAssignmentEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentCourseAssignmentEntity",
                table: "StudentCourseAssignmentEntity");

            migrationBuilder.RenameTable(
                name: "StudentCourseAssignmentEntity",
                newName: "StudentCourseAssignments");

            migrationBuilder.RenameIndex(
                name: "IX_StudentCourseAssignmentEntity_CourseId",
                table: "StudentCourseAssignments",
                newName: "IX_StudentCourseAssignments_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentCourseAssignments",
                table: "StudentCourseAssignments",
                columns: new[] { "StudentId", "CourseId" });

            migrationBuilder.CreateTable(
                name: "LessonSeries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    SchoolId = table.Column<int>(type: "int", nullable: true),
                    CourseId = table.Column<int>(type: "int", nullable: true),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timezone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonSeries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonSeries_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonSeries_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonSeries_StudentEntities_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonSeries_TeacherEntities_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "TeacherEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LessonEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeriesId = table.Column<int>(type: "int", nullable: true),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    SchoolId = table.Column<int>(type: "int", nullable: true),
                    CourseId = table.Column<int>(type: "int", nullable: true),
                    ScheduledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: false),
                    Timezone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    StatusReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedByUserId = table.Column<int>(type: "int", nullable: true),
                    RescheduledFromId = table.Column<int>(type: "int", nullable: true),
                    SeriesInstanceIndex = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonEvents_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonEvents_LessonEvents_RescheduledFromId",
                        column: x => x.RescheduledFromId,
                        principalTable: "LessonEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonEvents_LessonSeries_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "LessonSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonEvents_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonEvents_StudentEntities_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonEvents_TeacherEntities_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "TeacherEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonEvents_UserEntities_ModifiedByUserId",
                        column: x => x.ModifiedByUserId,
                        principalTable: "UserEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LessonReschedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginalLessonId = table.Column<int>(type: "int", nullable: false),
                    NewLessonId = table.Column<int>(type: "int", nullable: false),
                    RescheduleReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RescheduleDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RescheduleByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonReschedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonReschedules_LessonEvents_NewLessonId",
                        column: x => x.NewLessonId,
                        principalTable: "LessonEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonReschedules_LessonEvents_OriginalLessonId",
                        column: x => x.OriginalLessonId,
                        principalTable: "LessonEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonReschedules_UserEntities_RescheduleByUserId",
                        column: x => x.RescheduleByUserId,
                        principalTable: "UserEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonEvents_CourseId",
                table: "LessonEvents",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonEvents_ModifiedByUserId",
                table: "LessonEvents",
                column: "ModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonEvents_RescheduledFromId",
                table: "LessonEvents",
                column: "RescheduledFromId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonEvents_SchoolId",
                table: "LessonEvents",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonEvents_SeriesId_SeriesInstanceIndex",
                table: "LessonEvents",
                columns: new[] { "SeriesId", "SeriesInstanceIndex" },
                unique: true,
                filter: "[SeriesId] IS NOT NULL AND [SeriesInstanceIndex] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LessonEvents_SeriesId_Status",
                table: "LessonEvents",
                columns: new[] { "SeriesId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LessonEvents_StudentId",
                table: "LessonEvents",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonEvents_TeacherId_StudentId_ScheduledAtUtc",
                table: "LessonEvents",
                columns: new[] { "TeacherId", "StudentId", "ScheduledAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_LessonReschedules_NewLessonId",
                table: "LessonReschedules",
                column: "NewLessonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonReschedules_OriginalLessonId",
                table: "LessonReschedules",
                column: "OriginalLessonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonReschedules_RescheduleByUserId",
                table: "LessonReschedules",
                column: "RescheduleByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonSeries_CourseId",
                table: "LessonSeries",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonSeries_SchoolId",
                table: "LessonSeries",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonSeries_StudentId",
                table: "LessonSeries",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonSeries_TeacherId_StudentId_StartDate",
                table: "LessonSeries",
                columns: new[] { "TeacherId", "StudentId", "StartDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourseAssignments_Courses_CourseId",
                table: "StudentCourseAssignments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourseAssignments_StudentEntities_StudentId",
                table: "StudentCourseAssignments",
                column: "StudentId",
                principalTable: "StudentEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourseAssignments_Courses_CourseId",
                table: "StudentCourseAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourseAssignments_StudentEntities_StudentId",
                table: "StudentCourseAssignments");

            migrationBuilder.DropTable(
                name: "LessonReschedules");

            migrationBuilder.DropTable(
                name: "LessonEvents");

            migrationBuilder.DropTable(
                name: "LessonSeries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentCourseAssignments",
                table: "StudentCourseAssignments");

            migrationBuilder.RenameTable(
                name: "StudentCourseAssignments",
                newName: "StudentCourseAssignmentEntity");

            migrationBuilder.RenameIndex(
                name: "IX_StudentCourseAssignments_CourseId",
                table: "StudentCourseAssignmentEntity",
                newName: "IX_StudentCourseAssignmentEntity_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentCourseAssignmentEntity",
                table: "StudentCourseAssignmentEntity",
                columns: new[] { "StudentId", "CourseId" });

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourseAssignmentEntity_Courses_CourseId",
                table: "StudentCourseAssignmentEntity",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourseAssignmentEntity_StudentEntities_StudentId",
                table: "StudentCourseAssignmentEntity",
                column: "StudentId",
                principalTable: "StudentEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
