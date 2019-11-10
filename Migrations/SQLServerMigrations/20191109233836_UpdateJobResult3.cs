using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace worker_sqlexpress.Migrations.SQLServerMigrations
{
    public partial class UpdateJobResult3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateExecuted",
                table: "JobResult",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateExecuted",
                table: "JobResult");
        }
    }
}
