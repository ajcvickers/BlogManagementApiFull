﻿using System.Data.Entity;

namespace WebApi_Net7;

public class BlogsContext : DbContext
{
    public BlogsContext()
        : base("name=Blogs")
    {
    }
        
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
}