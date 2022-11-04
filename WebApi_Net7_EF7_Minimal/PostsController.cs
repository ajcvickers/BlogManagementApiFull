using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi_Net7_EF6;

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

        using (var transaction = _context.Database.BeginTransaction())
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            transaction.Rollback();
        }

        return Ok(post);
    }

    [HttpPut("api/posts")]
    public async Task<ActionResult<Post>> UpdatePost(Post post)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        using (var transaction = _context.Database.BeginTransaction())
        {
            _context.Entry(post).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            transaction.Rollback();
        }

        return Ok(post);
    }

    [HttpDelete("api/posts/{id}")]
    public async Task<ActionResult> DeletePost(int id)
    {
        // var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
        //
        // if (post == null)
        // {
        //     return NotFound();
        // }
        //
        // using (var transaction = _context.Database.BeginTransaction())
        // {
        //     _context.Posts.Remove(post);
        //     await _context.SaveChangesAsync();
        //
        //     transaction.Rollback();
        // }
        //
        // return Ok();

        int rowsAffected;

        using (var transaction = _context.Database.BeginTransaction())
        {
            rowsAffected = await _context.Posts.Where(p => p.Id == id).ExecuteDeleteAsync();

            transaction.Rollback();
        }

        return rowsAffected == 0
            ? NotFound()
            : Ok();
    }

    [HttpPut("api/posts/archive")]
    public async Task<ActionResult> ArchivePosts(string blogName, int priorToYear)
    {
        var priorToDateTime = new DateTime(priorToYear, 1, 1);

        using (var transaction = _context.Database.BeginTransaction())
        {
            // var posts = await _context.Posts
            //     .Include(p => p.Blog.Account)
            //     .Where(p => p.Blog.Name == blogName
            //                 && p.PublishedOn < priorToDateTime
            //                 && !p.Archived)
            //     .ToListAsync();
            //
            // foreach (var post in posts)
            // {
            //     var accountDetails = JsonConvert.DeserializeObject<AccountDetails>(post.Blog.Account.Details)!;
            //     if (!accountDetails.IsPremium)
            //     {
            //         post.Archived = true;
            //         post.Banner = $"This post was published in {post.PublishedOn.Year} and has been archived.";
            //         post.Title += $" ({post.PublishedOn.Year})";
            //     }
            // }
            //
            // await _context.SaveChangesAsync();

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

            transaction.Rollback();
        }

        return Ok();
    }
}
