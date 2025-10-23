using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLearningPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserLearningPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    WordId = table.Column<int>(type: "int", nullable: false),
                    GroupIndex = table.Column<int>(type: "int", nullable: false),
                    LastExampleIndex = table.Column<int>(type: "int", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLearningPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLearningPositions_VocabularyWords_WordId",
                        column: x => x.WordId,
                        principalTable: "VocabularyWords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "VocabularyCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 19, 1, 36, 267, DateTimeKind.Unspecified).AddTicks(8397), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 19, 1, 36, 267, DateTimeKind.Unspecified).AddTicks(9066), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 19, 1, 36, 267, DateTimeKind.Unspecified).AddTicks(9074), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 19, 1, 36, 269, DateTimeKind.Unspecified).AddTicks(3831), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 2,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 19, 1, 36, 269, DateTimeKind.Unspecified).AddTicks(3840), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 3,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 19, 1, 36, 269, DateTimeKind.Unspecified).AddTicks(3843), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 4,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 19, 1, 36, 269, DateTimeKind.Unspecified).AddTicks(3846), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyWords",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 19, 1, 36, 270, DateTimeKind.Unspecified).AddTicks(7986), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyWords",
                keyColumn: "Id",
                keyValue: 2,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 19, 1, 36, 270, DateTimeKind.Unspecified).AddTicks(7995), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyWords",
                keyColumn: "Id",
                keyValue: 3,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 19, 1, 36, 270, DateTimeKind.Unspecified).AddTicks(7999), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningPosition_UserId_WordId",
                table: "UserLearningPositions",
                columns: new[] { "UserId", "WordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningPositions_WordId",
                table: "UserLearningPositions",
                column: "WordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLearningPositions");

            migrationBuilder.UpdateData(
                table: "VocabularyCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 56, 31, 43, DateTimeKind.Unspecified).AddTicks(149), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 56, 31, 43, DateTimeKind.Unspecified).AddTicks(813), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 56, 31, 43, DateTimeKind.Unspecified).AddTicks(819), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 56, 31, 44, DateTimeKind.Unspecified).AddTicks(6611), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 2,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 56, 31, 44, DateTimeKind.Unspecified).AddTicks(6619), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 3,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 56, 31, 44, DateTimeKind.Unspecified).AddTicks(6622), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 4,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 56, 31, 44, DateTimeKind.Unspecified).AddTicks(6624), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyWords",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 56, 31, 46, DateTimeKind.Unspecified).AddTicks(478), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyWords",
                keyColumn: "Id",
                keyValue: 2,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 56, 31, 46, DateTimeKind.Unspecified).AddTicks(486), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyWords",
                keyColumn: "Id",
                keyValue: 3,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 23, 17, 56, 31, 46, DateTimeKind.Unspecified).AddTicks(489), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
