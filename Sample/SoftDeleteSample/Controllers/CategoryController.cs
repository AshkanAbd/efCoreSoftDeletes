using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoftDeleteSample.Models;

namespace SoftDeleteSample.Controllers
{
    public class CategoryController : ControllerBase
    {
        private SoftDeleteSampleDbContext Context;

        public CategoryController(SoftDeleteSampleDbContext context)
        {
            Context = context;
        }

        [HttpGet]
        [Route("category")]
        [ApiExplorerSettings(GroupName = "V1")]
        public async Task<ActionResult> Index()
        {
            var categories = await Context.Categories
                .AsNoTracking()
                .OrderByDescending(category => category.Id)
                .Select(category => new {
                    category.Id,
                    category.Name,
                }).ToListAsync();

            return Ok(categories);
        }

        [HttpPost]
        [Route("category")]
        [ApiExplorerSettings(GroupName = "V1")]
        public async Task<ActionResult> Store(
            [FromForm] string name
        )
        {
            var category = new Category {
                Name = name,
            };

            await Context.AddAsync(category);
            await Context.SaveChangesAsync();
            return Ok("category created.");
        }

        [HttpGet]
        [Route("category/{id}")]
        [ApiExplorerSettings(GroupName = "V1")]
        public async Task<ActionResult> Show(
            long id
        )
        {
            var category = await Context.Categories
                .FindAsync(id);

            if (category == null) {
                return NotFound("category notfound.");
            }

            await Context.Entry(category)
                .Collection(c => c.Posts)
                .LoadAsync();

            return Ok(new {
                category.Id,
                category.Name,
                Post = category.Posts
                    .Select(post => new {
                        post.Id,
                        post.Title,
                        post.Description,
                    })
            });
        }

        [HttpPut]
        [Route("category/{id}")]
        [ApiExplorerSettings(GroupName = "V1")]
        public async Task<ActionResult> Update(
            long id,
            [FromForm] string name
        )
        {
            var category = await Context.Categories
                .FindAsync(id);

            if (category == null) {
                return NotFound("category notfound.");
            }

            category.Name = name;

            await Context.SaveChangesAsync();
            return Ok("category saved.");
        }

        [HttpDelete]
        [Route("category/{id}")]
        [ApiExplorerSettings(GroupName = "V1")]
        public async Task<ActionResult> Destroy(
            long id
        )
        {
            var category = await Context.Categories
                .FindAsync(id);

            if (category == null) {
                return NotFound("category notfound.");
            }

            await Context.RemoveAsync(category);
            await Context.SaveChangesAsync();

            return Ok("category removed.");
        }
    }
}