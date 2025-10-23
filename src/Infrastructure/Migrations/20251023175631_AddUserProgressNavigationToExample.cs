using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProgressNavigationToExample : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VocabularyExampleId",
                table: "UserExampleProgress",
                type: "int",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_UserExampleProgress_VocabularyExampleId",
                table: "UserExampleProgress",
                column: "VocabularyExampleId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserExampleProgress_VocabularyExamples_VocabularyExampleId",
                table: "UserExampleProgress",
                column: "VocabularyExampleId",
                principalTable: "VocabularyExamples",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserExampleProgress_VocabularyExamples_VocabularyExampleId",
                table: "UserExampleProgress");

            migrationBuilder.DropIndex(
                name: "IX_UserExampleProgress_VocabularyExampleId",
                table: "UserExampleProgress");

            migrationBuilder.DropColumn(
                name: "VocabularyExampleId",
                table: "UserExampleProgress");

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
        }
    }
}
