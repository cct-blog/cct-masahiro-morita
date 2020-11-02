using Microsoft.EntityFrameworkCore.Migrations;

namespace blazorTest.Server.Migrations
{
    public partial class AddHandleNameToApplicationUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HandleName",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HandleName",
                table: "AspNetUsers");
        }
    }
}
