using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hamalba.Migrations
{
    /// <inheritdoc />
    public partial class Prijava : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KorisnikOglasi",
                columns: table => new
                {
                    PrijavaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OglasId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KorisnikOglasi", x => x.PrijavaId);
                    table.ForeignKey(
                        name: "FK_KorisnikOglasi_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KorisnikOglasi_Oglasi_OglasId",
                        column: x => x.OglasId,
                        principalTable: "Oglasi",
                        principalColumn: "OglasId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_KorisnikOglasi_OglasId",
                table: "KorisnikOglasi",
                column: "OglasId");

            migrationBuilder.CreateIndex(
                name: "IX_KorisnikOglasi_UserId",
                table: "KorisnikOglasi",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KorisnikOglasi");
        }
    }
}
