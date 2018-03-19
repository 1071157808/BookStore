using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace BookStore.ProjectionBuilder.Postgres.Database.Migrations
{
    public partial class AddStreamVersionsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StreamVersions",
                columns: table => new
                {
                    StreamName = table.Column<string>(maxLength: 64, nullable: false),
                    Version = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamVersions", x => x.StreamName);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StreamVersions");
        }
    }
}
