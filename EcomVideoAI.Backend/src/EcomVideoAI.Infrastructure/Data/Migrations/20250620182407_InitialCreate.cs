using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcomVideoAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "videos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    text_prompt = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    input_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    resolution = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    duration_seconds = table.Column<int>(type: "integer", nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    video_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    thumbnail_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    freepik_task_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    freepik_image_task_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_videos", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_videos_created_at",
                table: "videos",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_videos_freepik_image_task_id",
                table: "videos",
                column: "freepik_image_task_id",
                unique: true,
                filter: "freepik_image_task_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_videos_freepik_task_id",
                table: "videos",
                column: "freepik_task_id",
                unique: true,
                filter: "freepik_task_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_videos_status",
                table: "videos",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_videos_user_id",
                table: "videos",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "videos");
        }
    }
}
