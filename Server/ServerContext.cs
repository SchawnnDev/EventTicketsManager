using System;
using System.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Server
{
	public class ServerContext : IdentityDbContext<IdentityUser>
    {

        private static DbContextOptions Options { get; set; }

        public ServerContext() : base(Options) { }

        public ServerContext(DbContextOptions options) : base(options)
        {
            Options = options;
        }
        
		public DbSet<SaveableEvent> Events { get; set; }

        public DbSet<SaveableEventUser> EventUsers { get; set; }

        public DbSet<SaveableTicket> Tickets { get; set; }

        public DbSet<SaveableTicketScan> TicketScans { get; set; }

        public DbSet<SaveableTicketUserMail> TicketUserMails { get; set; }

        public DbSet<SaveableLog> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("public");
			/* modelBuilder.Entity<SaveableOperation>()
				.HasOptional(a => a.Invoice)
				.WithOptionalDependent()
				.WillCascadeOnDelete(true); */
			base.OnModelCreating(builder);
		}
	}
}
