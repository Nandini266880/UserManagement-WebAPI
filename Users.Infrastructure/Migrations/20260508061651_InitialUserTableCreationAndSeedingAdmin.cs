using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialUserTableCreationAndSeedingAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "Role", "UpdatedAt" },
                values: new object[] { new Guid("540e371f-82c8-4d92-a6c4-a1c9364b4c30"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@gmail.com", "Admin User", true, "$2a$11$qQjHfNmmEDuj/Uq7nHoe8uoyE.NDP0iOAaI7XR6Cv2ikQoFvu.PQK", 1, null });

            migrationBuilder.CreateIndex(
                name: "Idx_On_User_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
