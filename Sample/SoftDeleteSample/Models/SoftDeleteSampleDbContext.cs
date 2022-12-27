using Microsoft.EntityFrameworkCore;

namespace SoftDeleteSample.Models
{
    public class SoftDeleteSampleDbContext : SoftDeletes.Core.DbContext
    {
        protected SoftDeleteSampleDbContext()
        {
        }

        public SoftDeleteSampleDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // For category
            modelBuilder.Entity<Category>()
                .HasQueryFilter(category => category.DeletedAt == null);

            // For post
            modelBuilder.Entity<Post>()
                .HasQueryFilter(post => post.DeletedAt == null);
            modelBuilder.Entity<Post>()
                .HasOne(post => post.Category)
                .WithMany(category => category.Posts)
                .HasForeignKey(post => post.CategoryId);

            // For comment
            modelBuilder.Entity<Comment>()
                .HasQueryFilter(comment => comment.DeletedAt == null);
            modelBuilder.Entity<Comment>()
                .HasOne(comment => comment.Post)
                .WithMany(post => post.Comments)
                .HasForeignKey(comment => comment.PostId);
        }
    }
}