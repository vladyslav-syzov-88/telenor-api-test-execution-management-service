using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telenor.Api.TestExecutionManagement.Infrastructure.Migrations
{
	/// <inheritdoc />
	public partial class InitialCreate : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Projects",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					JiraProjectId = table.Column<int>(type: "int", nullable: false),
					Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Projects", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "TestCases",
				columns: table => new
				{
					Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
					ProjectId = table.Column<int>(type: "int", nullable: false),
					JiraIssueKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
					JiraIssueId = table.Column<int>(type: "int", nullable: true),
					Summary = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TestCases", x => x.Id);
					table.ForeignKey(
						name: "FK_TestCases_Projects_ProjectId",
						column: x => x.ProjectId,
						principalTable: "Projects",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Versions",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					ProjectId = table.Column<int>(type: "int", nullable: false),
					JiraVersionId = table.Column<int>(type: "int", nullable: true),
					Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
					IsReleased = table.Column<bool>(type: "bit", nullable: false),
					ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Versions", x => x.Id);
					table.ForeignKey(
						name: "FK_Versions_Projects_ProjectId",
						column: x => x.ProjectId,
						principalTable: "Projects",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "TestCycles",
				columns: table => new
				{
					Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
					ProjectId = table.Column<int>(type: "int", nullable: false),
					VersionId = table.Column<int>(type: "int", nullable: false),
					Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
					Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
					EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TestCycles", x => x.Id);
					table.ForeignKey(
						name: "FK_TestCycles_Projects_ProjectId",
						column: x => x.ProjectId,
						principalTable: "Projects",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_TestCycles_Versions_VersionId",
						column: x => x.VersionId,
						principalTable: "Versions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "CycleFolders",
				columns: table => new
				{
					Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
					CycleId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
					Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
					SortOrder = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_CycleFolders", x => x.Id);
					table.ForeignKey(
						name: "FK_CycleFolders_TestCycles_CycleId",
						column: x => x.CycleId,
						principalTable: "TestCycles",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "TestExecutions",
				columns: table => new
				{
					Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
					CycleId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
					FolderId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
					TestCaseId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
					VersionId = table.Column<int>(type: "int", nullable: false),
					ProjectId = table.Column<int>(type: "int", nullable: false),
					StatusId = table.Column<int>(type: "int", nullable: false),
					AssignedTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TestExecutions", x => x.Id);
					table.ForeignKey(
						name: "FK_TestExecutions_CycleFolders_FolderId",
						column: x => x.FolderId,
						principalTable: "CycleFolders",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_TestExecutions_Projects_ProjectId",
						column: x => x.ProjectId,
						principalTable: "Projects",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_TestExecutions_TestCases_TestCaseId",
						column: x => x.TestCaseId,
						principalTable: "TestCases",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_TestExecutions_TestCycles_CycleId",
						column: x => x.CycleId,
						principalTable: "TestCycles",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_TestExecutions_Versions_VersionId",
						column: x => x.VersionId,
						principalTable: "Versions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "ExecutionHistory",
				columns: table => new
				{
					Id = table.Column<long>(type: "bigint", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					ExecutionId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
					StatusId = table.Column<int>(type: "int", nullable: false),
					Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
					ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
					ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ExecutionHistory", x => x.Id);
					table.ForeignKey(
						name: "FK_ExecutionHistory_TestExecutions_ExecutionId",
						column: x => x.ExecutionId,
						principalTable: "TestExecutions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_CycleFolders_CycleId",
				table: "CycleFolders",
				column: "CycleId");

			migrationBuilder.CreateIndex(
				name: "IX_ExecutionHistory_ExecutionId_ChangedAt",
				table: "ExecutionHistory",
				columns: new[] { "ExecutionId", "ChangedAt" });

			migrationBuilder.CreateIndex(
				name: "IX_Projects_JiraProjectId",
				table: "Projects",
				column: "JiraProjectId",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_TestCases_JiraIssueKey",
				table: "TestCases",
				column: "JiraIssueKey");

			migrationBuilder.CreateIndex(
				name: "IX_TestCases_ProjectId",
				table: "TestCases",
				column: "ProjectId");

			migrationBuilder.CreateIndex(
				name: "IX_TestCycles_ProjectId",
				table: "TestCycles",
				column: "ProjectId");

			migrationBuilder.CreateIndex(
				name: "IX_TestCycles_VersionId",
				table: "TestCycles",
				column: "VersionId");

			migrationBuilder.CreateIndex(
				name: "IX_TestExecutions_CycleId_FolderId",
				table: "TestExecutions",
				columns: new[] { "CycleId", "FolderId" });

			migrationBuilder.CreateIndex(
				name: "IX_TestExecutions_CycleId_StatusId",
				table: "TestExecutions",
				columns: new[] { "CycleId", "StatusId" });

			migrationBuilder.CreateIndex(
				name: "IX_TestExecutions_FolderId",
				table: "TestExecutions",
				column: "FolderId");

			migrationBuilder.CreateIndex(
				name: "IX_TestExecutions_ProjectId",
				table: "TestExecutions",
				column: "ProjectId");

			migrationBuilder.CreateIndex(
				name: "IX_TestExecutions_TestCaseId",
				table: "TestExecutions",
				column: "TestCaseId");

			migrationBuilder.CreateIndex(
				name: "IX_TestExecutions_VersionId",
				table: "TestExecutions",
				column: "VersionId");

			migrationBuilder.CreateIndex(
				name: "IX_Versions_ProjectId",
				table: "Versions",
				column: "ProjectId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "ExecutionHistory");

			migrationBuilder.DropTable(
				name: "TestExecutions");

			migrationBuilder.DropTable(
				name: "CycleFolders");

			migrationBuilder.DropTable(
				name: "TestCases");

			migrationBuilder.DropTable(
				name: "TestCycles");

			migrationBuilder.DropTable(
				name: "Versions");

			migrationBuilder.DropTable(
				name: "Projects");
		}
	}
}