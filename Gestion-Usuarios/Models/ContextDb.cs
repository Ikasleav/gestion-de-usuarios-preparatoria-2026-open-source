using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Gestion_Usuarios.Models
{
	public class ContextDb : DbContext
	{
		public ContextDb(DbContextOptions<ContextDb> options) : base(options)
		{

		}
		// public DbSet<Usuario> Usuarios { get; set; }
		public DbSet<ManagementUser> ManagementUsers { get; set; }
	}
