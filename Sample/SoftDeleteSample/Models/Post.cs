using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SoftDeleteSample.Models
{
    public class Post : SoftDeletes.ModelTools.ModelExtension
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public long CategoryId { get; set; }

        public Category Category { get; set; }
        public List<Comment> Comments { get; set; }

        public override async Task OnSoftDeleteAsync(SoftDeletes.Core.DbContext context,
            CancellationToken cancellationToken = default)
        {
            var taskList = new List<Task> {
                context.RemoveRangeAsync(Comments,cancellationToken)
            };

            await Task.WhenAll(taskList);
        }

        public override void OnSoftDelete(SoftDeletes.Core.DbContext context)
        {
            context.RemoveRange(Comments);
        }

        public override async Task LoadRelationsAsync(SoftDeletes.Core.DbContext context,
            CancellationToken cancellationToken = default)
        {
            var taskList = new List<Task> {
                context.Entry(this)
                    .Collection(post => post.Comments)
                    .LoadAsync()
            };

            await Task.WhenAll(taskList);
        }

        public override void LoadRelations(SoftDeletes.Core.DbContext context)
        {
            context.Entry(this)
                .Collection(post => post.Comments)
                .Load();
        }
    }
}