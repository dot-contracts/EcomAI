using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcomVideoAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoAspectRatio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "aspect_ratio",
                table: "videos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "style",
                table: "videos",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "aspect_ratio",
                table: "videos");

            migrationBuilder.DropColumn(
                name: "style",
                table: "videos");
        }
    }
}
