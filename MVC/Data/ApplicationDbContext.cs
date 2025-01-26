using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OmSaiModels.Admin;


namespace OmSaiServices_v3.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options)
		: base(options)
		{
		}

		public DbSet<ApplicationUser> AspNetUsers { get; set; }
		//public DbSet<IdentityRole> AspNetRoles { get; set; }
		public DbSet<IdentityUserRole<string>> AspNetUserRoles { get; set; }
		public DbSet<Role> AspNetRoles { get; set; }


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Configure primary key for IdentityUserRole
			modelBuilder.Entity<IdentityUserRole<string>>()
				.HasKey(ur => new { ur.UserId, ur.RoleId });  // Composite key using UserId and RoleId
		}

	}

	public class ApplicationUser
	{
		[Key]
		public string Id { get; set; }
		public string Email { get; set; }
		public string UserName { get; set; }		
		public string PasswordHash { get; set; }
		public bool EmailConfirmed { get; set; }
	}
}
