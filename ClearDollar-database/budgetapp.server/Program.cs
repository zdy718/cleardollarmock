using BudgetAppCSCE361.Data;
using BudgetApp.Server.Accessors;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure database
            var cs = builder.Configuration.GetConnectionString("Default");
            builder.Services.AddDbContext<AppDbContext>(options => 
                options.UseSqlServer(cs));

            // Add services
            builder.Services.AddControllers();
            
            builder.Services.AddScoped<ITagAccessor, TagAccessor>();
            builder.Services.AddScoped<ITransactionAccessor, TransactionAccessor>();
            builder.Services.AddScoped<IBudgetAccessor, BudgetAccessor>();
            builder.Services.AddScoped<IUserAccessor, UserAccessor>();

            var app = builder.Build();

            // Middleware
            app.UseDefaultFiles();
            app.MapStaticAssets();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            
            // Routes
            app.MapControllers();
            app.MapFallbackToFile("/index.html");

            Console.WriteLine("Server Running - Connected to SQL Server ✅");
            app.Run();
        }
    }
}