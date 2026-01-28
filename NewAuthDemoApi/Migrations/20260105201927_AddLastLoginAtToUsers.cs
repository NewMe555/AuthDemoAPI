using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthDemoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddLastLoginAtToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "LastLoginAt", "PasswordHash" },
                values: new object[] { null, "$2a$11$/fH0JHV3xcS.GAbRX43GOuOO6kZrO3pK564.gGPG6ucxxy9mryoDe" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$edBo.5ZeGYC7Yl17ctP0buPN9OmI7VUd2nJSQRSCoTJuCvzCIaCPG");
        }
    }
}
