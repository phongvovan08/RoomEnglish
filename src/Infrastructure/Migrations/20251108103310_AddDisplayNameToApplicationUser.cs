using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayNameToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "VocabularyCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 11, 8, 10, 33, 9, 671, DateTimeKind.Unspecified).AddTicks(996), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 11, 8, 10, 33, 9, 671, DateTimeKind.Unspecified).AddTicks(2285), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 11, 8, 10, 33, 9, 671, DateTimeKind.Unspecified).AddTicks(2298), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 11, 8, 10, 33, 9, 673, DateTimeKind.Unspecified).AddTicks(6760), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 2,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 11, 8, 10, 33, 9, 673, DateTimeKind.Unspecified).AddTicks(6779), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 3,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 11, 8, 10, 33, 9, 673, DateTimeKind.Unspecified).AddTicks(6782), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 4,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 11, 8, 10, 33, 9, 673, DateTimeKind.Unspecified).AddTicks(6785), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyWords",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 11, 8, 10, 33, 9, 675, DateTimeKind.Unspecified).AddTicks(8216), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyWords",
                keyColumn: "Id",
                keyValue: 2,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 11, 8, 10, 33, 9, 675, DateTimeKind.Unspecified).AddTicks(8237), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyWords",
                keyColumn: "Id",
                keyValue: 3,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 11, 8, 10, 33, 9, 675, DateTimeKind.Unspecified).AddTicks(8241), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "VocabularyCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 25, 8, 38, 5, 347, DateTimeKind.Unspecified).AddTicks(2373), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 25, 8, 38, 5, 347, DateTimeKind.Unspecified).AddTicks(5555), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 25, 8, 38, 5, 347, DateTimeKind.Unspecified).AddTicks(5581), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 25, 8, 38, 5, 349, DateTimeKind.Unspecified).AddTicks(3000), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 2,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 25, 8, 38, 5, 349, DateTimeKind.Unspecified).AddTicks(3009), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 3,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 25, 8, 38, 5, 349, DateTimeKind.Unspecified).AddTicks(3012), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyExamples",
                keyColumn: "Id",
                keyValue: 4,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 25, 8, 38, 5, 349, DateTimeKind.Unspecified).AddTicks(3014), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyWords",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 25, 8, 38, 5, 350, DateTimeKind.Unspecified).AddTicks(5728), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyWords",
                keyColumn: "Id",
                keyValue: 2,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 25, 8, 38, 5, 350, DateTimeKind.Unspecified).AddTicks(5769), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "VocabularyWords",
                keyColumn: "Id",
                keyValue: 3,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2025, 10, 25, 8, 38, 5, 350, DateTimeKind.Unspecified).AddTicks(5772), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
