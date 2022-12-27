using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SoftDeleteSample.Models
{
    public class Category : SoftDeletes.ModelTools.ModelExtension
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public List<Post> Posts { get; set; }

        public override async Task OnSoftDeleteAsync(SoftDeletes.Core.DbContext context,
            CancellationToken cancellationToken = default)
        {
            var taskList = new List<Task> {
                context.RemoveRangeAsync(Posts, cancellationToken)
            };

            await Task.WhenAll(taskList);
        }

        public override void OnSoftDelete(SoftDeletes.Core.DbContext context)
        {
            context.RemoveRange(Posts);
        }

        public override async Task LoadRelationsAsync(SoftDeletes.Core.DbContext context,
            CancellationToken cancellationToken = default)
        {
            var taskList = new List<Task> {
                context.Entry(this)
                    .Collection(category => category.Posts)
                    .LoadAsync(cancellationToken)
            };

            await Task.WhenAll(taskList);
        }

        public override void LoadRelations(SoftDeletes.Core.DbContext context)
        {
            context.Entry(this)
                .Collection(category => category.Posts)
                .Load();
        }
    }
}