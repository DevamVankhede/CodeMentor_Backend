using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeMentorAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRoadmapModelsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Custom_Editors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    AuthorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Language = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Settings = table.Column<string>(type: "TEXT", nullable: false),
                    Features = table.Column<string>(type: "TEXT", nullable: false),
                    DefaultCode = table.Column<string>(type: "TEXT", nullable: false),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsTemplate = table.Column<bool>(type: "INTEGER", nullable: false),
                    UsageCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Custom_Editors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Custom_Editors_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Roadmaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Difficulty = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    EstimatedDuration = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Topics = table.Column<string>(type: "TEXT", nullable: false),
                    Goals = table.Column<string>(type: "TEXT", nullable: false),
                    AuthorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roadmaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roadmaps_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Roadmap_Enrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoadmapId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Progress = table.Column<int>(type: "INTEGER", nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roadmap_Enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roadmap_Enrollments_Roadmaps_RoadmapId",
                        column: x => x.RoadmapId,
                        principalTable: "Roadmaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Roadmap_Enrollments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roadmap_Steps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoadmapId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    StepType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Resources = table.Column<string>(type: "TEXT", nullable: true),
                    Requirements = table.Column<string>(type: "TEXT", nullable: true),
                    EstimatedHours = table.Column<int>(type: "INTEGER", nullable: false),
                    IsOptional = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roadmap_Steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roadmap_Steps_Roadmaps_RoadmapId",
                        column: x => x.RoadmapId,
                        principalTable: "Roadmaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User_Step_Progress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StepId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Progress = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Step_Progress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Step_Progress_Roadmap_Steps_StepId",
                        column: x => x.StepId,
                        principalTable: "Roadmap_Steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User_Step_Progress_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Custom_Editors_AuthorId",
                table: "Custom_Editors",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Custom_Editors_CreatedAt",
                table: "Custom_Editors",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Custom_Editors_IsPublic",
                table: "Custom_Editors",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_Custom_Editors_IsTemplate",
                table: "Custom_Editors",
                column: "IsTemplate");

            migrationBuilder.CreateIndex(
                name: "IX_Custom_Editors_Language",
                table: "Custom_Editors",
                column: "Language");

            migrationBuilder.CreateIndex(
                name: "IX_Roadmap_Enrollments_EnrolledAt",
                table: "Roadmap_Enrollments",
                column: "EnrolledAt");

            migrationBuilder.CreateIndex(
                name: "IX_Roadmap_Enrollments_Progress",
                table: "Roadmap_Enrollments",
                column: "Progress");

            migrationBuilder.CreateIndex(
                name: "IX_Roadmap_Enrollments_RoadmapId",
                table: "Roadmap_Enrollments",
                column: "RoadmapId");

            migrationBuilder.CreateIndex(
                name: "IX_Roadmap_Enrollments_UserId",
                table: "Roadmap_Enrollments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Roadmap_Steps_OrderIndex",
                table: "Roadmap_Steps",
                column: "OrderIndex");

            migrationBuilder.CreateIndex(
                name: "IX_Roadmap_Steps_RoadmapId",
                table: "Roadmap_Steps",
                column: "RoadmapId");

            migrationBuilder.CreateIndex(
                name: "IX_Roadmap_Steps_StepType",
                table: "Roadmap_Steps",
                column: "StepType");

            migrationBuilder.CreateIndex(
                name: "IX_Roadmaps_AuthorId",
                table: "Roadmaps",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Roadmaps_Category",
                table: "Roadmaps",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Roadmaps_CreatedAt",
                table: "Roadmaps",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Roadmaps_Difficulty",
                table: "Roadmaps",
                column: "Difficulty");

            migrationBuilder.CreateIndex(
                name: "IX_Roadmaps_Status",
                table: "Roadmaps",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_User_Step_Progress_IsCompleted",
                table: "User_Step_Progress",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_User_Step_Progress_Progress",
                table: "User_Step_Progress",
                column: "Progress");

            migrationBuilder.CreateIndex(
                name: "IX_User_Step_Progress_StepId",
                table: "User_Step_Progress",
                column: "StepId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Step_Progress_UserId",
                table: "User_Step_Progress",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Custom_Editors");

            migrationBuilder.DropTable(
                name: "Roadmap_Enrollments");

            migrationBuilder.DropTable(
                name: "User_Step_Progress");

            migrationBuilder.DropTable(
                name: "Roadmap_Steps");

            migrationBuilder.DropTable(
                name: "Roadmaps");
        }
    }
}
