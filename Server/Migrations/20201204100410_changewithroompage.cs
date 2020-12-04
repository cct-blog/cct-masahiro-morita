using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace blazorTest.Server.Migrations
{
    public partial class changewithroompage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_UserIdId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Threads_AspNetUsers_UserIdId",
                table: "Threads");

            migrationBuilder.DropForeignKey(
                name: "FK_UserInfoInRooms_AspNetUsers_UserIdId",
                table: "UserInfoInRooms");

            migrationBuilder.RenameColumn(
                name: "UserIdId",
                table: "UserInfoInRooms",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserInfoInRooms_UserIdId",
                table: "UserInfoInRooms",
                newName: "IX_UserInfoInRooms_ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "UserIdId",
                table: "Threads",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Threads_UserIdId",
                table: "Threads",
                newName: "IX_Threads_ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "UserIdId",
                table: "Posts",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_UserIdId",
                table: "Posts",
                newName: "IX_Posts_ApplicationUserId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "UserInfoInRooms",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "UserInfoInRooms",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsumedTime",
                table: "PersistedGrants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PersistedGrants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "PersistedGrants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DeviceCodes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "DeviceCodes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersistedGrants_SubjectId_SessionId_Type",
                table: "PersistedGrants",
                columns: new[] { "SubjectId", "SessionId", "Type" });

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_ApplicationUserId",
                table: "Posts",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Threads_AspNetUsers_ApplicationUserId",
                table: "Threads",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfoInRooms_AspNetUsers_ApplicationUserId",
                table: "UserInfoInRooms",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_ApplicationUserId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Threads_AspNetUsers_ApplicationUserId",
                table: "Threads");

            migrationBuilder.DropForeignKey(
                name: "FK_UserInfoInRooms_AspNetUsers_ApplicationUserId",
                table: "UserInfoInRooms");

            migrationBuilder.DropIndex(
                name: "IX_PersistedGrants_SubjectId_SessionId_Type",
                table: "PersistedGrants");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "UserInfoInRooms");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "UserInfoInRooms");

            migrationBuilder.DropColumn(
                name: "ConsumedTime",
                table: "PersistedGrants");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PersistedGrants");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "PersistedGrants");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "DeviceCodes");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "DeviceCodes");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "UserInfoInRooms",
                newName: "UserIdId");

            migrationBuilder.RenameIndex(
                name: "IX_UserInfoInRooms_ApplicationUserId",
                table: "UserInfoInRooms",
                newName: "IX_UserInfoInRooms_UserIdId");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Threads",
                newName: "UserIdId");

            migrationBuilder.RenameIndex(
                name: "IX_Threads_ApplicationUserId",
                table: "Threads",
                newName: "IX_Threads_UserIdId");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Posts",
                newName: "UserIdId");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_ApplicationUserId",
                table: "Posts",
                newName: "IX_Posts_UserIdId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_UserIdId",
                table: "Posts",
                column: "UserIdId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Threads_AspNetUsers_UserIdId",
                table: "Threads",
                column: "UserIdId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfoInRooms_AspNetUsers_UserIdId",
                table: "UserInfoInRooms",
                column: "UserIdId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
