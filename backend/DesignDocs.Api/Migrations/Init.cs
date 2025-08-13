using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DesignDocs.Api.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gin;");
            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RepoPath = table.Column<string>(nullable: false),
                    Slug = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    Team = table.Column<string>(nullable: true),
                    Owner = table.Column<string>(nullable: true),
                    Approver = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    Scope = table.Column<int>(nullable: false),
                    Tags = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    CurrentCommit = table.Column<string>(nullable: true),
                    BaseReviewCommit = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Documents");
        }
    }
}
