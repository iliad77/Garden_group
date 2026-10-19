using DotNetEnv;
using Garden_group.Repositories;
using Garden_group.Services;
using MongoDB.Driver;

namespace Garden_group
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Env.Load();

            var builder = WebApplication.CreateBuilder(args);

            string connectionString = Environment.GetEnvironmentVariable("MONGODB_URI")
                ?? throw new InvalidOperationException("MONGODB_URI is missing from .env");
            string databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE")
                ?? throw new InvalidOperationException("MONGODB_DATABASE is missing from .env");

            // MongoDB setup (Luca Mulders)
            // One MongoClient for the whole app, it's thread-safe and manages its own connection pool
            builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
            builder.Services.AddSingleton(sp =>
                sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
           
            //Ticket seach and filter (Chris)
            builder.Services.AddScoped<TicketSearchService>();
            builder.Services.AddScoped<TicketFilterService>();

            // Stateless and wraps a thread-safe collection, so one instance can be shared
            builder.Services.AddSingleton<ITicketRepository, TicketRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepo>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days.  https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
