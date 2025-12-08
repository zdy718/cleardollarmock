namespace BudgetApp.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // Register mock accessors so Controllers can receive ITagAccessor/ITransactionAccessor
            builder.Services.AddSingleton<BudgetApp.Server.Accessors.ITagAccessor, BudgetApp.Server.Accessors.MockTagAccessor>();
            builder.Services.AddSingleton<BudgetApp.Server.Accessors.ITransactionAccessor, BudgetApp.Server.Accessors.MockTransactionAccessor>();

            var app = builder.Build();

            app.UseDefaultFiles();
            app.MapStaticAssets();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.MapFallbackToFile("/index.html");


            Console.WriteLine("Server Running");

            app.Run();


        }
    }
}
