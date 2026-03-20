using Microsoft.EntityFrameworkCore;
using Telenor.Api.TestExecutionManagement.Core.Entities;
using Version = Telenor.Api.TestExecutionManagement.Core.Entities.Version;

namespace Telenor.Api.TestExecutionManagement.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<Project> Projects => Set<Project>();
	public DbSet<Version> Versions => Set<Version>();
	public DbSet<TestCycle> TestCycles => Set<TestCycle>();
	public DbSet<CycleFolder> CycleFolders => Set<CycleFolder>();
	public DbSet<TestCase> TestCases => Set<TestCase>();
	public DbSet<TestExecution> TestExecutions => Set<TestExecution>();
	public DbSet<ExecutionHistory> ExecutionHistory => Set<ExecutionHistory>();
	public DbSet<ConnectTenant> ConnectTenants => Set<ConnectTenant>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Project>(e =>
		{
			e.HasKey(p => p.Id);
			e.HasIndex(p => p.JiraProjectId).IsUnique();
			e.Property(p => p.Name).HasMaxLength(256);
		});

		modelBuilder.Entity<Version>(e =>
		{
			e.HasKey(v => v.Id);
			e.Property(v => v.Name).HasMaxLength(256);
			e.HasOne(v => v.Project).WithMany(p => p.Versions).HasForeignKey(v => v.ProjectId);
		});

		modelBuilder.Entity<TestCycle>(e =>
		{
			e.HasKey(c => c.Id);
			e.Property(c => c.Id).HasMaxLength(128);
			e.Property(c => c.Name).HasMaxLength(512);
			e.Property(c => c.Status).HasConversion<string>().HasMaxLength(64);
			e.HasOne(c => c.Project).WithMany(p => p.TestCycles).HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.Restrict);
			e.HasOne(c => c.Version).WithMany(v => v.TestCycles).HasForeignKey(c => c.VersionId).OnDelete(DeleteBehavior.Restrict);
		});

		modelBuilder.Entity<CycleFolder>(e =>
		{
			e.HasKey(f => f.Id);
			e.Property(f => f.Id).HasMaxLength(128);
			e.Property(f => f.CycleId).HasMaxLength(128);
			e.Property(f => f.Name).HasMaxLength(256);
			e.HasOne(f => f.Cycle).WithMany(c => c.Folders).HasForeignKey(f => f.CycleId);
		});

		modelBuilder.Entity<TestCase>(e =>
		{
			e.HasKey(t => t.Id);
			e.Property(t => t.Id).HasMaxLength(128);
			e.Property(t => t.JiraIssueKey).HasMaxLength(64);
			e.Property(t => t.Summary).HasMaxLength(1024);
			e.HasIndex(t => t.JiraIssueKey);
			e.HasOne(t => t.Project).WithMany(p => p.TestCases).HasForeignKey(t => t.ProjectId);
		});

		modelBuilder.Entity<TestExecution>(e =>
		{
			e.HasKey(x => x.Id);
			e.Property(x => x.Id).HasMaxLength(128);
			e.Property(x => x.CycleId).HasMaxLength(128);
			e.Property(x => x.FolderId).HasMaxLength(128);
			e.Property(x => x.TestCaseId).HasMaxLength(128);
			e.Property(x => x.StatusId).HasConversion<int>();
			e.HasIndex(x => new { x.CycleId, x.FolderId });
			e.HasIndex(x => new { x.CycleId, x.StatusId });
			e.HasOne(x => x.Cycle).WithMany(c => c.Executions).HasForeignKey(x => x.CycleId).OnDelete(DeleteBehavior.Restrict);
			e.HasOne(x => x.Folder).WithMany(f => f.Executions).HasForeignKey(x => x.FolderId).OnDelete(DeleteBehavior.Restrict);
			e.HasOne(x => x.TestCase).WithMany(t => t.Executions).HasForeignKey(x => x.TestCaseId).OnDelete(DeleteBehavior.Restrict);
			e.HasOne(x => x.Version).WithMany().HasForeignKey(x => x.VersionId).OnDelete(DeleteBehavior.Restrict);
			e.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
		});

		modelBuilder.Entity<ExecutionHistory>(e =>
		{
			e.HasKey(h => h.Id);
			e.Property(h => h.ExecutionId).HasMaxLength(128);
			e.Property(h => h.StatusId).HasConversion<int>();
			e.Property(h => h.ChangedBy).HasMaxLength(256);
			e.HasIndex(h => new { h.ExecutionId, h.ChangedAt });
			e.HasOne(h => h.Execution).WithMany(x => x.History).HasForeignKey(h => h.ExecutionId);
		});

		modelBuilder.Entity<ConnectTenant>(e =>
		{
			e.HasKey(t => t.Id);
			e.HasIndex(t => t.ClientKey).IsUnique();
			e.Property(t => t.ClientKey).HasMaxLength(256);
			e.Property(t => t.SharedSecret).HasMaxLength(512);
			e.Property(t => t.BaseUrl).HasMaxLength(512);
			e.Property(t => t.DisplayUrl).HasMaxLength(512);
			e.Property(t => t.ProductType).HasMaxLength(128);
			e.Property(t => t.Description).HasMaxLength(1024);
		});
	}
}