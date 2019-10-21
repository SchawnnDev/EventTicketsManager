using System;
using System.Data.Entity;

namespace Server
{
	public class ServerContext : DbContext
	{

		public ServerContext() : base("Server")
		{
			Configuration.LazyLoadingEnabled = false;
		}

		public DbSet<SaveableUser> Users { get; set; }

		public DbSet<SaveableAccount> Accounts { get; set; }


		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.HasDefaultSchema("public");
			/* modelBuilder.Entity<SaveableOperation>()
				.HasOptional(a => a.Invoice)
				.WithOptionalDependent()
				.WillCascadeOnDelete(true); */
			base.OnModelCreating(modelBuilder);
		}
	}
}
