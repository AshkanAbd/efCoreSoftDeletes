using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using SoftDeletes.Core;

namespace SoftDeletes.ModelTools
{
    public interface ISoftDelete
    {
        /// <summary>
        /// To save the entity soft delete date and time.
        /// </summary>
        [DefaultValue(null)]
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Check for the entity will soft delete or force delete.
        /// </summary>
        protected internal bool ForceDelete { get; set; }

        /// <summary>
        /// Set the entity to force delete.
        /// </summary>
        /// <param name="shouldDelete">True for force delete, False for soft delete.</param>
        internal void SetForceDelete(bool shouldDelete = true)
        {
            ForceDelete = shouldDelete;
        }

        /// <summary>
        /// Check the entity will force delete on soft delete
        /// </summary>
        /// <returns>True if will force delete, False if will soft delete.</returns>
        public virtual bool IsForceDelete()
        {
            return ForceDelete;
        }

        /// <summary>
        /// Will call on soft deleting the entity.
        /// </summary>
        /// <remarks>
        /// Remove related data of the entity that you want to delete on soft deleting the entity.
        /// </remarks>
        /// <param name="context">Application DbContext</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task OnSoftDeleteAsync([NotNull] DbContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Will call on soft deleting the entity.
        /// </summary>
        /// <remarks>
        /// Remove related data of the entity that you want to delete on soft deleting the entity.
        /// </remarks>
        /// <param name="context">Application DbContext</param>
        public void OnSoftDelete([NotNull] DbContext context);

        /// <summary>
        /// Will call on soft deleting the entity.
        /// </summary>
        /// <remarks>
        /// Load related data of the entity that you want to delete on soft deleting the entity.
        /// </remarks>
        /// <param name="context">Application DbContext</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task LoadRelationsAsync([NotNull] DbContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Will call on soft deleting the entity.
        /// </summary>
        /// <remarks>
        /// Load related data of the entity that you want to delete on soft deleting the entity.
        /// </remarks>
        /// <param name="context">Application DbContext</param>
        public void LoadRelations([NotNull] DbContext context);
    }
}