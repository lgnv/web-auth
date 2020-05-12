using Microsoft.EntityFrameworkCore.Migrations;

namespace PhotosApp.Migrations.UsersDb
{
    public partial class Paid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Paid",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Paid",
                table: "AspNetUsers");
        }
    }
}
