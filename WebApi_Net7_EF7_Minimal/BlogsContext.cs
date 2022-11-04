using Microsoft.EntityFrameworkCore;

namespace WebApi_Net7_EF6;

// public class BlogsContext : DbContext
// {
//     public BlogsContext()
//         : base("name=Blogs")
//     {
//     }
//
//     public DbSet<Blog> Blogs { get; set; }
//     public DbSet<Post> Posts { get; set; }
//     public DbSet<Account> Accounts { get; set; }
//
//     protected override void OnModelCreating(DbModelBuilder modelBuilder)
//     {
//         modelBuilder
//             .Entity<Blog>()
//             .HasIndex(e => e.Name);
//     }
// }

public class BlogsContext : DbContext
{
    public BlogsContext(DbContextOptions<BlogsContext> options)
        : base(options)
    {
    }

    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Account> Accounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Blog>()
            .HasIndex(e => e.Name);

        modelBuilder
            .Entity<Account>()
            .OwnsOne(p => p.Details)
            .ToJson();
    }
}
