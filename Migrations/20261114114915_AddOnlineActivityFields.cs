using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusKonnect.Migrations
{
    /// <inheritdoc />
    public partial class AddOnlineActivityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActivityType",
                table: "Activities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MeetingLink",
                table: "Activities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Platform",
                table: "Activities",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivityType",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "MeetingLink",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "Activities");
        }
    }
}
