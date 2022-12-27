using System.Threading;
using System.Threading.Tasks;

namespace SoftDeleteSample.Models
{
    public class Comment : SoftDeletes.ModelTools.ModelExtension
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public long PostId { get; set; }
        public Post Post { get; set; }

        public override Task OnSoftDeleteAsync(SoftDeletes.Core.DbContext context,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override void OnSoftDelete(SoftDeletes.Core.DbContext context)
        {
        }

        public override Task LoadRelationsAsync(SoftDeletes.Core.DbContext context,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override void LoadRelations(SoftDeletes.Core.DbContext context)
        {
        }
    }
}