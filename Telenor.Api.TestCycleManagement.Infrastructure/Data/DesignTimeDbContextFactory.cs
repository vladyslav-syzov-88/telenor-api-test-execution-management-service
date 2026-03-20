using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Telenor.Api.TestExecutionManagement.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
	public AppDbContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
		optionsBuilder.UseSqlServer("Server=localhost;Database=TestExecutionManagement;Trusted_Connection=True;TrustServerCertificate=True;");
		return new AppDbContext(optionsBuilder.Options);
	}
}