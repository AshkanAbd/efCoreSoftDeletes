using System;
using System.Threading;
using System.Threading.Tasks;
using SoftDeletes.Core;

namespace SoftDeletes.ModelTools
{
    public abstract class ModelExtension : ITimestamps, ISoftDelete
    {
        /// <inheritdoc />
        public DateTime CreatedAt { get; set; }

        /// <inheritdoc />
        public DateTime UpdatedAt { get; set; }

        /// <inheritdoc />
        public DateTime? DeletedAt { get; set; }

        /// <inheritdoc />
        bool ISoftDelete.ForceDelete { get; set; }

        /// <inheritdoc />
        public abstract Task OnSoftDeleteAsync(DbContext context, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public abstract void OnSoftDelete(DbContext context);

        /// <inheritdoc />
        public abstract Task LoadRelationsAsync(DbContext context, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public abstract void LoadRelations(DbContext context);
    }
}