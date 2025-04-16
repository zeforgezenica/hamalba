using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hamalba.Migrations
{
    /// <inheritdoc />
    public partial class AdminArhiva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Arhiviran",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Arhiviran",
                table: "AspNetUsers");
        }
    }
}
