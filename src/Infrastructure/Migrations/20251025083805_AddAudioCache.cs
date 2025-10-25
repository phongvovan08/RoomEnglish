using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAudioCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AudioCaches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Voice = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Rate = table.Column<double>(type: "float", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AudioData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SizeBytes = table.Column<int>(type: "int", nullable: false),
                    CacheKeyHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    HitCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastAccessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioCaches", x => x.Id);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_AudioCache_CacheKeyHash",
                table: "AudioCaches",
                column: "CacheKeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AudioCache_ExpiresAt",
                table: "AudioCaches",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_AudioCache_HitCount_LastAccessedAt",
                table: "AudioCaches",
                columns: new[] { "HitCount", "LastAccessedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AudioCaches");

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
        }
    }
}
