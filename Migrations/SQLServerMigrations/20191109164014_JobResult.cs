using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace worker_sqlexpress.Migrations.SQLServerMigrations
{
    public partial class JobResult : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastExecuted",
                table: "Job");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRun",
                table: "Job",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "NextExecution",
                table: "Job",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "JobResult",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobName = table.Column<string>(maxLength: 100, nullable: false),
                    ResultJson = table.Column<string>(nullable: false),
                    Runtime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobResult", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobResult");

            migrationBuilder.DropColumn(
                name: "LastRun",
                table: "Job");

            migrationBuilder.DropColumn(
                name: "NextExecution",
                table: "Job");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastExecuted",
                table: "Job",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
