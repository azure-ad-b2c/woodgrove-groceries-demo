using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WoodGroveGroceriesWebApplication.EntityFramework;

namespace WoodGroveGroceriesWebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var webHost = BuildWebHost(args);

            using (var serviceScope = webHost.Services.CreateScope())
            {
                var serviceProvider = serviceScope.ServiceProvider;
                var dbContext = serviceProvider.GetRequiredService<WoodGroveGroceriesDbContext>();
                var dbContextInitializationOptions = serviceProvider.GetRequiredService<IOptions<DbContextInitializationOptions>>().Value;

                WoodGroveGroceriesDbContextInitializer.InitializeAsync(dbContext, dbContextInitializationOptions)
                    .Wait();
            }

            webHost.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
