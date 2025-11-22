using Microsoft.EntityFrameworkCore;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        
        public DbSet<Users> Users { get; set; }
        public DbSet<Projects> Projects { get; set; }
        public DbSet<Tables> Tables { get; set; }
        public DbSet<Columns> Columns { get; set; }
        public DbSet<Rows> Rows { get; set; }
        public DbSet<Relationships> Relationships { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            
            modelBuilder.Entity<Rows>().Property(r => r.Data).HasColumnType("jsonb");

            modelBuilder.Entity<Users>()
                  .HasKey(u => u.User_id);
            modelBuilder.Entity<Users>().Property(u => u.User_id)
                .ValueGeneratedOnAdd(); 

            modelBuilder.Entity<Projects>()
                .HasKey(p => p.Project_id);
            modelBuilder.Entity<Projects>().Property(p => p.Project_id)
                .ValueGeneratedOnAdd(); 

            modelBuilder.Entity<Tables>()
                .HasKey(t => t.Table_id);
            modelBuilder.Entity<Tables>().Property(t => t.Table_id)
                .ValueGeneratedOnAdd(); 

            modelBuilder.Entity<Columns>()
                .HasKey(c => c.Column_id);
            modelBuilder.Entity<Columns>().Property(c => c.Column_id)
                .ValueGeneratedOnAdd(); 

            modelBuilder.Entity<Rows>()
                .HasKey(r => r.Row_id);
            modelBuilder.Entity<Rows>().Property(r => r.Row_id)
                .ValueGeneratedOnAdd(); 

            modelBuilder.Entity<Relationships>()
                .HasKey(r => r.RelationshipId);
            modelBuilder.Entity<Relationships>().Property(r => r.RelationshipId)
                .ValueGeneratedOnAdd(); 
            modelBuilder
                .Entity<Users>()
                .HasMany(u => u.Projects)
                .WithOne(p => p.Users)
                .HasForeignKey(p => p.User_id)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder
                .Entity<Projects>()
                .HasMany(p => p.Tables)
                .WithOne(t => t.Projects)
                .HasForeignKey(t => t.Project_id)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder
                .Entity<Tables>()
                .HasMany(t => t.Columns)
                .WithOne(c => c.Tables)
                .HasForeignKey(c => c.Table_id)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder
                .Entity<Tables>()
                .HasMany(t => t.Rows)
                .WithOne(r => r.Table)
                .HasForeignKey(r => r.Table_id)
                .OnDelete(DeleteBehavior.Cascade);

            

            
            
            
            modelBuilder
                .Entity<Relationships>()
                .HasOne(r => r.PrimaryTable)
                .WithMany()
                .HasForeignKey(r => r.PrimaryTableId)
                .OnDelete(DeleteBehavior.NoAction); 

            
            
            modelBuilder
                .Entity<Relationships>()
                .HasOne(r => r.ForeignTable)
                .WithMany()
                .HasForeignKey(r => r.ForeignTableId)
                .OnDelete(DeleteBehavior.NoAction); 

            
            
            modelBuilder
                .Entity<Relationships>()
                .HasOne(r => r.PrimaryColumn)
                .WithMany()
                .HasForeignKey(r => r.PrimaryColumnId)
                .OnDelete(DeleteBehavior.Cascade); 

            
            
            modelBuilder
                .Entity<Relationships>()
                .HasOne(r => r.ForeignColumn)
                .WithMany()
                .HasForeignKey(r => r.ForeignColumnId)
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}