namespace WoodGroveGroceriesWebApplication
{
    using EntityFramework;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Models.Settings;
    using Services;

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
                var host = serviceProvider.GetRequiredService<HostService>();

                WoodGroveGroceriesDbContextInitializer.InitializeAsync(dbContext, dbContextInitializationOptions, host)
                    .Wait();
            }

            webHost.Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
        }
    }
}