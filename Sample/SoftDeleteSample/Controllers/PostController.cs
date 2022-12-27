using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoftDeleteSample.Models;

namespace SoftDeleteSample.Controllers
{
    public class PostController : ControllerBase
    {
        private SoftDeleteSampleDbContext Context;

        public PostController(SoftDeleteSampleDbContext context)
        {
            Context = context;
        }

        [HttpGet]
        [Route("post")]
        [ApiExplorerSettings(GroupName = "V1")]
        public async Task<ActionResult> Index(
            [FromQuery] long? categoryId
        )
        {
            var postQuery = Context.Posts
                .AsNoTracking()
                .OrderByDescending(post => post.Id)
                .AsQueryable();

            if (categoryId != null) {
                postQuery = postQuery.Where(post => post.CategoryId == categoryId)
                    .AsQueryable();
            }

            var posts = await postQuery
                .Select(post => new {
                    post.Id,
                    post.Title,
                    post.Description,
                    Category = new {
                        post.Category.Id,
                        post.Category.Name,
                    }
                }).ToListAsync();

            return Ok(posts);
        }

        [HttpPost]
        [Route("post")]
        [ApiExplorerSettings(GroupName = "V1")]
        public async Task<ActionResult> Store(
            [FromForm] string title,
            [FromForm] string description,
            [FromForm] long categoryId
        )
        {
            var post = new Post {
                Title = title,
                Description = description,
                CategoryId = categoryId
            };

            await Context.AddAsync(post);
            await Context.SaveChangesAsync();

            return Ok("post created.");
        }

        [HttpGet]
        [Route("post/{id}")]
        [ApiExplorerSettings(GroupName = "V1")]
        public async Task<ActionResult> Show(
            long id
        )
        {
            var post = await Context.Posts
                .FindAsync(id);

            if (post == null) {
                return NotFound("post notfound");
            }

            await Context.Entry(post)
                .Collection(p => p.Comments)
                .LoadAsync();

            await Context.Entry(post)
                .Reference(p => p.Category)
                .LoadAsync();

            return Ok(new {
                post.Id,
                post.Title,
                post.Description,
                Category = new {
                    post.Category.Id,
                    post.Category.Name,
                },
                Comments = post.Comments
                    .Select(comment => new {
                        comment.Id,
                        comment.Content,
                    })
            });
        }

        [HttpPut]
        [Route("post/{id}")]
        [ApiExplorerSettings(GroupName = "V1")]
        public async Task<ActionResult> Update(
            long id,
            [FromForm] string title,
            [FromForm] string description,
            [FromForm] long categoryId
        )
        {
            var post = await Context.Posts
                .FindAsync(id);

            if (post == null) {
                return NotFound("post notfound");
            }

            post.Title = title;
            post.Description = description;
            post.CategoryId = categoryId;

            await Context.SaveChangesAsync();

            return Ok("post saved.");
        }

        [HttpDelete]
        [Route("post/{id}")]
        [ApiExplorerSettings(GroupName = "V1")]
        public async Task<ActionResult> Destroy(
            long id
        )
        {
            var post = await Context.Posts
                .FindAsync(id);

            if (post == null) {
                return NotFound("post notfound");
            }

            await Context.RemoveAsync(post);
            await Context.SaveChangesAsync();

            return Ok("post removed.");
        }
    }
}