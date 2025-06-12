using Lost_and_Found.Models.Entites;
using Microsoft.EntityFrameworkCore;

namespace Lost_and_Found.Models
{
    public class DataConnection : DbContext
    {
        public DataConnection(DbContextOptions<DataConnection> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<LostCard> LostCards { get; set; }
        public DbSet<LostPhone> LostPhones { get; set; }
        public DbSet<FindCard> FindCards { get; set; }
        public DbSet<FindPhone> FindPhones { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<PasswordResetCode> PasswordResetCodes { get; set; }




        protected override void OnModelCreating(ModelBuilder op)
        {
            base.OnModelCreating(op);

            op.Entity<User>(o =>
            {
                o.HasIndex(o => o.Email).IsUnique();
                o.HasIndex(o => o.PhoneNumber).IsUnique();
                o.HasIndex(o => o.CardID).IsUnique();

                o.HasMany(x => x.LostCards)                 // One To Many relation with table lost cards
                .WithOne(x => x.User)
                .HasForeignKey(x => x.ForiegnKey_UserEmail)
                .HasPrincipalKey(o => o.Email)
                .OnDelete(DeleteBehavior.Restrict);
            });

            op.Entity<User>(o =>                            // One To Many relation with table lost phones
            {
                o.HasMany(x => x.LostPhones)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.ForiegnKey_UserEmail)
                .HasPrincipalKey(o => o.Email)
                .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
