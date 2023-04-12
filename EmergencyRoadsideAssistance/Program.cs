using Dapper;
using EmergencyRoadsideAssistance.Services;

namespace EmergencyRoadsideAssistance
{
    public class Program
    {
        public static void Main()
        {
            var builder = WebApplication.CreateBuilder();

            // Add services to the container.
            var AllowAllOriginsAndHeaders = "allowAllOriginsAndHeaders";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: AllowAllOriginsAndHeaders, builder => { builder.AllowAnyOrigin(); builder.AllowAnyMethod(); builder.AllowAnyHeader(); });
            });
             
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<IRoadsideAssistanceService, RoadsideAssistanceService>();
            builder.Services.AddScoped<IDBService, DBService>();

            //postgres
            DefaultTypeMap.MatchNamesWithUnderscores = true;


            var app = builder.Build();

            // Configure the HTTP request pipeline.            
            app.UseSwagger();
            app.UseSwaggerUI();

            //app.UseHttpsRedirection();
            app.UseCors(AllowAllOriginsAndHeaders);

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}