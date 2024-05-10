using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HotelBackend.Tests.Base {
  public abstract class BaseRealDBTests {
      protected readonly IConfiguration _configuration;

      public BaseRealDBTests() 
      {
          var testProjectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
          _configuration = new ConfigurationBuilder()
                          .SetBasePath(testProjectPath)
                          .AddJsonFile("testsettings.json")
                          .Build();
      }
      protected DbContextOptions<MainDbContext> GetPostgresTestOptions() 
      {
          var connectionString = _configuration.GetConnectionString("TestDatabase");

          return new DbContextOptionsBuilder<MainDbContext>()
              .UseNpgsql(connectionString, options => options.UseNetTopologySuite())
              .Options;
      }

      protected void TruncateTestDB() {
          var options = GetPostgresTestOptions();

          using var context = new MainDbContext(options);

          // TODO: this won't work in production just like that
          //       Permissions!
          context.Database.EnsureDeleted();
          context.Database.Migrate();
      }
  }
}