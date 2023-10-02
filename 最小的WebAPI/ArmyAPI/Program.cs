using Microsoft.EntityFrameworkCore;
using ArmyAPI.Models;
using ArmyAPI.Commons;
using ArmyAPI.Data;

namespace ArmyAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddSingleton<CustomService>();
			builder.Services.AddSingleton<WriteLog>();

            builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddDbContext<TodoContext>(opt =>
				opt.UseInMemoryDatabase("TodoList"));

			builder.Services.AddDbContext<UserContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			var app = builder.Build();

            var customService = app.Services.GetRequiredService<CustomService>();
            WriteLog.Initialize(customService);


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();

			app.UseStaticHostEnviroment();


			app.MapControllers();

			app.Run();
		}
	}
}