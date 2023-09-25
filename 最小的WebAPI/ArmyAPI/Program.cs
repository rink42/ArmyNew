using Microsoft.EntityFrameworkCore;
using ArmyAPI.Models;
using ArmyAPI.Commons;

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


			app.MapControllers();

			app.Run();
		}
	}
}