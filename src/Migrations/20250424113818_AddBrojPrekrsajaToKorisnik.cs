using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hamalba.Migrations
{
    /// <inheritdoc />
    public partial class AddBrojPrekrsajaToKorisnik : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BrojPrekrsaja",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrojPrekrsaja",
                table: "AspNetUsers");
        }
    }
}
