using Microsoft.EntityFrameworkCore;

namespace ArmyAPI.Models
{
	class TodoDb : DbContext
	{
		public TodoDb(DbContextOptions<TodoDb> options)
			: base(options) { }

		public DbSet<TodoItem> Todos => Set<TodoItem>();
	}
}
