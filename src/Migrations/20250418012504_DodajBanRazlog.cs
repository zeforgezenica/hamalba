using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hamalba.Migrations
{
    /// <inheritdoc />
    public partial class DodajBanRazlog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BanRazlog",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BanRazlog",
                table: "AspNetUsers");
        }
    }
}
