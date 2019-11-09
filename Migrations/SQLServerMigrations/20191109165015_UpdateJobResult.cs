using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace worker_sqlexpress.Migrations.SQLServerMigrations
{
    public partial class UpdateJobResult : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "Runtime",
                table: "JobResult",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Runtime",
                table: "JobResult",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeSpan));
        }
    }
}
