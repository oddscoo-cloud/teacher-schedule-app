using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentFinance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentLessonCharges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    SourceLessonId = table.Column<int>(type: "int", nullable: false),
                    ChargeType = table.Column<int>(type: "int", nullable: false),
                    LessonStartsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ChargedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RecordedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentLessonCharges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentLessonCharges_StudentEntities_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentLessonCharges_UserEntities_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "UserEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidLessonCount = table.Column<int>(type: "int", nullable: false),
                    PaidAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RecordedByUserId = table.Column<int>(type: "int", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentPayments_StudentEntities_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentPayments_UserEntities_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "UserEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentPriceHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RecordedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentPriceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentPriceHistory_StudentEntities_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentPriceHistory_UserEntities_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "UserEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentPaymentAdjustments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    AmountDelta = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidLessonCountDelta = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RecordedByUserId = table.Column<int>(type: "int", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentPaymentAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentPaymentAdjustments_StudentPayments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "StudentPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentPaymentAdjustments_UserEntities_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "UserEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentLessonCharges_RecordedByUserId",
                table: "StudentLessonCharges",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLessonCharges_StudentId_SourceLessonId",
                table: "StudentLessonCharges",
                columns: new[] { "StudentId", "SourceLessonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentPaymentAdjustments_PaymentId_IdempotencyKey",
                table: "StudentPaymentAdjustments",
                columns: new[] { "PaymentId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentPaymentAdjustments_RecordedByUserId",
                table: "StudentPaymentAdjustments",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPayments_RecordedByUserId",
                table: "StudentPayments",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPayments_StudentId_IdempotencyKey",
                table: "StudentPayments",
                columns: new[] { "StudentId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentPriceHistory_RecordedByUserId",
                table: "StudentPriceHistory",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPriceHistory_StudentId_EffectiveFrom",
                table: "StudentPriceHistory",
                columns: new[] { "StudentId", "EffectiveFrom" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentLessonCharges");

            migrationBuilder.DropTable(
                name: "StudentPaymentAdjustments");

            migrationBuilder.DropTable(
                name: "StudentPriceHistory");

            migrationBuilder.DropTable(
                name: "StudentPayments");
        }
    }
}
