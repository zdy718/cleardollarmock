using BudgetApp.Server.Accessors;
using BudgetApp.Server.Data; // Add this namespace
using Microsoft.EntityFrameworkCore; // Add this namespace

namespace BudgetApp.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            // 1. Register the Database Context
            builder.Services.AddDbContext<BudgetDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2. Change Dependencies from Singleton Mock to Scoped SQL Accessors
            // Note: DB services usually use AddScoped, not AddSingleton
            builder.Services.AddScoped<ITagAccessor, SqlTagAccessor>();
            builder.Services.AddScoped<ITransactionAccessor, SqlTransactionAccessor>();

            var app = builder.Build();

            app.UseDefaultFiles();
            app.MapStaticAssets();

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.MapFallbackToFile("/index.html");

            Console.WriteLine("Server Running");

            app.Run();
        }
    }
}