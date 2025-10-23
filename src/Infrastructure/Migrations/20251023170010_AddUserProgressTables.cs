using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProgressTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserCategoryProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    WordsStudied = table.Column<int>(type: "int", nullable: false),
                    WordsMastered = table.Column<int>(type: "int", nullable: false),
                    ExamplesCompleted = table.Column<int>(type: "int", nullable: false),
                    CompletionPercentage = table.Column<int>(type: "int", nullable: false),
                    AverageAccuracy = table.Column<double>(type: "float", nullable: false),
                    TotalMinutesSpent = table.Column<int>(type: "int", nullable: false),
                    SessionsCompleted = table.Column<int>(type: "int", nullable: false),
                    FirstStudiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastStudiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsMastered = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCategoryProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCategoryProgress_VocabularyCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "VocabularyCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserExampleProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ExampleId = table.Column<int>(type: "int", nullable: false),
                    TotalAttempts = table.Column<int>(type: "int", nullable: false),
                    CorrectAttempts = table.Column<int>(type: "int", nullable: false),
                    BestAccuracy = table.Column<int>(type: "int", nullable: false),
                    AverageAccuracy = table.Column<double>(type: "float", nullable: false),
                    FastestTimeSeconds = table.Column<int>(type: "int", nullable: false),
                    TotalTimeSeconds = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsMastered = table.Column<bool>(type: "bit", nullable: false),
                    FirstAttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastAttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserExampleProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserExampleProgress_VocabularyExamples_ExampleId",
                        column: x => x.ExampleId,
                        principalTable: "VocabularyExamples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "VocabularyCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 0, 10, 21, DateTimeKind.Unspecified).AddTicks(9986), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 0, 10, 22, DateTimeKind.Unspecified).AddTicks(694), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 0, 10, 22, DateTimeKind.Unspecified).AddTicks(701), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 0, 10, 24, DateTimeKind.Unspecified).AddTicks(158), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 2,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 0, 10, 24, DateTimeKind.Unspecified).AddTicks(172), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 3,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 0, 10, 24, DateTimeKind.Unspecified).AddTicks(174), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 4,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 0, 10, 24, DateTimeKind.Unspecified).AddTicks(177), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyWords",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 0, 10, 25, DateTimeKind.Unspecified).AddTicks(5398), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyWords",
                keyColumn: "Id",
                keyValue: 2,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 0, 10, 25, DateTimeKind.Unspecified).AddTicks(5409), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyWords",
                keyColumn: "Id",
                keyValue: 3,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 0, 10, 25, DateTimeKind.Unspecified).AddTicks(5412), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_UserCategoryProgress_CategoryId",
                table: "UserCategoryProgress",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCategoryProgress_UserId_CategoryId",
                table: "UserCategoryProgress",
                columns: new[] { "UserId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserExampleProgress_ExampleId",
                table: "UserExampleProgress",
                column: "ExampleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserExampleProgress_UserId_ExampleId",
                table: "UserExampleProgress",
                columns: new[] { "UserId", "ExampleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCategoryProgress");

            migrationBuilder.DropTable(
                name: "UserExampleProgress");

            migrationBuilder.UpdateData(
                table: "VocabularyCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 22, 10, 30, 20, 98, DateTimeKind.Unspecified).AddTicks(930), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 22, 10, 30, 20, 98, DateTimeKind.Unspecified).AddTicks(1480), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 22, 10, 30, 20, 98, DateTimeKind.Unspecified).AddTicks(1487), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 22, 10, 30, 20, 99, DateTimeKind.Unspecified).AddTicks(4055), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 2,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 22, 10, 30, 20, 99, DateTimeKind.Unspecified).AddTicks(4063), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 3,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 22, 10, 30, 20, 99, DateTimeKind.Unspecified).AddTicks(4065), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 4,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 22, 10, 30, 20, 99, DateTimeKind.Unspecified).AddTicks(4067), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyWords",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 22, 10, 30, 20, 100, DateTimeKind.Unspecified).AddTicks(5438), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyWords",
                keyColumn: "Id",
                keyValue: 2,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 22, 10, 30, 20, 100, DateTimeKind.Unspecified).AddTicks(5445), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyWords",
                keyColumn: "Id",
                keyValue: 3,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 22, 10, 30, 20, 100, DateTimeKind.Unspecified).AddTicks(5448), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
