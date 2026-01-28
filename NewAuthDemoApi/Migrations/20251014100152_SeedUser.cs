using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthDemoApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "PasswordHash", "RefreshToken", "RefreshTokenExpiryTime", "Username" },
                values: new object[] { 1, "$2a$11$86tmnCOI3ZWuK6VYJPtskOychkhsviq1lxcPu3VB.JlmwNx9R1WMq", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "vaibhavi" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
