using Microsoft.EntityFrameworkCore;

namespace Gestion_Usuarios.Models
{
	public class ContextDb : DbContext
	{
		public ContextDb(DbContextOptions<ContextDb> options) : base(options)
		{
		}

		// Entidades principales para operaciones CRUD rápidas
		public DbSet<ManagementUser> ManagementUsers { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<ManagementUser>()
				.HasKey(u => u.management_user_ID);
		}
	}
}