using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hamalba.Migrations
{
    /// <inheritdoc />
    public partial class AddBanTrajanje : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BanTrajanje",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BanTrajanje",
                table: "AspNetUsers");
        }
    }
}
