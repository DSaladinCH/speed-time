using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSaladin.SpeedTime.Migrations
{
    /// <inheritdoc />
    public partial class InitialDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Original generation
            //migrationBuilder.CreateTable(
            //    name: "TrackedTimes",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        TrackingStarted = table.Column<DateTime>(type: "TEXT", nullable: false),
            //        TrackingStopped = table.Column<DateTime>(type: "TEXT", nullable: false),
            //        Title = table.Column<string>(type: "TEXT", nullable: false),
            //        IsBreak = table.Column<bool>(type: "INTEGER", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TrackedTimes", x => x.Id);
            //    });

            // To handle already existing installs, we need to create the table if it does not exists yet
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS TrackedTimes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TrackingStarted TEXT NOT NULL,
                    TrackingStopped TEXT NOT NULL,
                    Title TEXT NOT NULL,
                    IsBreak INTEGER NOT NULL
                );
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackedTimes");
        }
    }
}
