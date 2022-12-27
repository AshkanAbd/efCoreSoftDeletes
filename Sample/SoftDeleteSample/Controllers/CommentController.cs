using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoftDeleteSample.Models;

namespace SoftDeleteSample.Controllers
{
    public class CommentController : ControllerBase
    {
        private SoftDeleteSampleDbContext Context;

        public CommentController(SoftDeleteSampleDbContext context)
        {
            Context = context;
        }

        [HttpGet]
        [Route("comment")]
        [ApiExplorerSettings(GroupName = "V1")]
        public async Task<ActionResult> Index(
            [FromQuery] long? postId
        )
        {
            var commentQuery = Context.Comments
                .AsNoTracking()
                .OrderByDescending(comment => comment.Id)
                .AsQueryable();

            if (postId != null) {
                commentQuery = commentQuery
                    .Where(comment => comment.PostId == postId);
            }

            var comments = await commentQuery
                .Select(comment => new {
                    comment.Id,
                    comment.Content,
                    post = new {
                        comment.Post.Id,
                        comment.Post.Title,
                        comment.Post.Description,
                    }
                }).ToListAsync();

            return Ok(comments);
        }

        [HttpPost]
        [Route("comment")]
        [ApiExplorerSettings(GroupName = "V1")]
        public async Task<ActionResult> Store(
            [FromForm] string content,
            [FromForm] long postId
        )
        {
            var comment = new Comment {
                Content = content,
                PostId = postId
            };

            await Context.AddAsync(comment);
            await Context.SaveChangesAsync();

            return Ok("comment created.");
        }

        [HttpPut]
        [Route("comment/{id}")]
        [ApiExplorerSettings(GroupName = "V1")]
        public async Task<ActionResult> Update(
            long id,
            [FromForm] string content,
            [FromForm] long postId
        )
        {
            var comment = await Context.Comments
                .FindAsync(id);

            if (comment == null) {
                return NotFound("comment notfound");
            }

            comment.Content = content;
            comment.PostId = postId;
            await Context.SaveChangesAsync();

            return Ok("comment saved.");
        }


        [HttpDelete]
        [Route("comment/{id}")]
        [ApiExplorerSettings(GroupName = "V1")]
        public async Task<ActionResult> Destroy(
            long id
        )
        {
            var comment = await Context.Comments
                .FindAsync(id);

            if (comment == null) {
                return NotFound("comment notfound");
            }

            await Context.RemoveAsync(comment);
            await Context.SaveChangesAsync();

            return Ok("comment removed.");
        }
    }
}