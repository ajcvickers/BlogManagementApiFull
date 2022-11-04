using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi_Net7_EF7_Minimal;

[ApiController]
public class PostsController : ControllerBase
{
    private readonly BlogsContext _context;

    public PostsController(BlogsContext context)
    {
        _context = context;
    }

    [HttpGet("api/posts")]
    public async Task<IEnumerable<Post>> GetPosts()
        => await _context.Posts
            .Include(p => p.Blog)
            .OrderBy(e => e.Id)
            .Take(10000)
            .AsNoTracking()
            .ToListAsync();

    [HttpGet("api/posts/{id}")]
    public async Task<ActionResult<Post>> GetPost(int id)
    {
        var post = await _context.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return post == null ? NotFound() : post;
    }

    [HttpPost("api/posts")]
    public async Task<ActionResult<Post>> InsertPost(Post post)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        return Ok(post);
    }

    [HttpPut("api/posts")]
    public async Task<ActionResult<Post>> UpdatePost(Post post)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _context.Entry(post).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(post);
    }

    [HttpDelete("api/posts/{id}")]
    public async Task<ActionResult> DeletePost(int id)
        => await _context.Posts.Where(p => p.Id == id).ExecuteDeleteAsync() == 0
            ? NotFound()
            : Ok();

    [HttpPut("api/posts/archive")]
    public async Task<ActionResult> ArchivePosts(string blogName, int priorToYear)
    {
        var priorToDateTime = new DateTime(priorToYear, 1, 1);

        var transaction = Benchmarking.Enabled ? (IDisposable)_context.Database.BeginTransaction() : new DummyDisposable();

        await _context.Posts
            .Where(
                p => p.Blog.Name == blogName
                    && p.Blog.Account.Details.IsPremium == false
                    && p.PublishedOn < priorToDateTime
                    && !p.Archived)
            .ExecuteUpdateAsync(
                updates => updates
                    .SetProperty(p => p.Title, p => p.Title + " (" + p.PublishedOn.Year + ")")
                    .SetProperty(p => p.Banner, p => "This post was published in " + p.PublishedOn.Year + " and has been archived.")
                    .SetProperty(p => p.Archived, true));

        transaction.Dispose();

        return Ok();
    }

    [HttpDelete("api/posts/delete")]
    public async Task<ActionResult> DeletePosts(string blogName, int priorToYear)
    {
        var priorToDateTime = new DateTime(priorToYear, 1, 1);

        var transaction = Benchmarking.Enabled ? (IDisposable)_context.Database.BeginTransaction() : new DummyDisposable();

        await _context.Posts
            .Where(
                p => p.Blog.Name == blogName
                    && p.Blog.Account.Details.IsPremium == false
                    && p.PublishedOn < priorToDateTime
                    && !p.Archived)
            .ExecuteDeleteAsync();

        transaction.Dispose();

        return Ok();
    }

    [HttpPost("api/posts/insert")]
    public async Task<ActionResult> InsertPosts()
    {
        var transaction = Benchmarking.Enabled ? (IDisposable)_context.Database.BeginTransaction() : new DummyDisposable();

        var posts = new List<Post>();
        for (var i = 0; i < 1000; i++)
        {
            posts.Add(
                new Post
                {
                    BlogId = 1,
                    PublishedOn = DateTime.UtcNow,
                    Title = "New Post",
                    Content = "Yadda Yadda Yadda"
                });
        }

        _context.Posts.AddRange(posts);

        await _context.SaveChangesAsync();

        transaction.Dispose();

        return Ok();
    }
}
