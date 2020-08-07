using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SoftDeletes.ModelTools;

namespace SoftDeletes.Core
{
    public class DbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        protected bool allowRestore = false;
        protected DbContext()
        {
        }

        public DbContext(DbContextOptions options) : base(options)
        {
        }

        /// <inheritdoc />
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            SetNewEntitiesTimestamps();

            SetModifiedEntitiesTimestamps();

            DetectSoftDeleteEntities();

            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        /// <inheritdoc />
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = new CancellationToken())
        {
            SetNewEntitiesTimestamps();

            SetModifiedEntitiesTimestamps();

            DetectSoftDeleteEntities();

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// Will call on saving changes.
        /// </summary>
        /// <remarks>
        /// Will set new entities CreatedAt and UpdatedAt to current date and time
        /// if they implement from ITimestamps interface.
        /// </remarks>
        /// <see cref="ITimestamps"/>
        protected virtual void SetNewEntitiesTimestamps()
        {
            var newRecords = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added && x.Entity is ITimestamps)
                .Select(x => x.Entity as ITimestamps);
            foreach (var record in newRecords) {
                if (record == null) continue;
                var nowDateTime = DateTime.Now;
                record.CreatedAt = nowDateTime;
                record.UpdatedAt = nowDateTime;
            }
        }

        /// <summary>
        /// Will call on saving changes.
        /// </summary>
        /// <remarks>
        /// Will set updated entities UpdatedAt to current date and time
        /// if they implement from ITimestamps interface.
        /// </remarks>
        /// <see cref="ITimestamps"/>
        protected virtual void SetModifiedEntitiesTimestamps()
        {
            var updatedRecords = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Modified && (x.Entity is ITimestamps || x.Entity is ISoftDelete))
                .Select(x => x.Entity);
            foreach (var record in updatedRecords) {
                if (record == null) continue;
                
                if(record is ISoftDelete && allowRestore == false)
                {
                    this.Entry(record).Property("DeletedAt").IsModified = false;
                }
                if (record is ITimestamps)
                {
                    
                    this.Entry(record).Property("CreatedAt").IsModified = false;
                    var r = (ITimestamps)record;
                    if (r == null) continue;
                    r.UpdatedAt = DateTime.Now;
                }
                
            }

            allowRestore = false;
        }

        /// <summary>
        /// Will call on saving changes.
        /// </summary>
        /// <remarks>
        /// Will set deleted entities DeletedAt to current date and time
        /// if they implement from ISoftDelete interface and should soft delete.
        /// </remarks>
        /// <see cref="ISoftDelete"/>
        protected virtual void DetectSoftDeleteEntities()
        {
            var deletedRecords = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Deleted && x.Entity is ISoftDelete)
                .Select(x => x);
            foreach (var record in deletedRecords) {
                if (((ISoftDelete) record.Entity).IsForceDelete()) continue;
                record.State = EntityState.Modified;
                ((ISoftDelete) record.Entity).DeletedAt = DateTime.Now;
            }
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database for not soft delete implemented models and soft delete them from database
        ///     for soft delete implemented models when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         NOTE: This method will soft delete entities that implements soft deletes and force delete entities that
        ///         soft delete not implemented.
        ///     </para>
        ///     <para>
        ///         If the entity is already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking the entity (rather than marking it as <see cref="EntityState.Deleted" />) since the
        ///         entity was previously added to the context and does not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="Microsoft.EntityFrameworkCore.DbContext.Attach(object)" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </remarks>
        /// <param name="entity"> The entity to remove. </param>
        /// <returns>
        ///     The <see cref="EntityEntry" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public override EntityEntry<TEntity> Remove<TEntity>([NotNull] TEntity entity) where TEntity : class
        {
            var result = base.Remove(entity);

            if (!(entity is ISoftDelete x)) return result;

            if (x.IsForceDelete()) return result;

            x.LoadRelations(this);

            x.OnSoftDelete(this);

            return result;
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database for not soft delete implemented models and soft delete them from database
        ///     for soft delete implemented models when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         NOTE: This method will soft delete entities that implements soft deletes and force delete entities that
        ///         soft delete not implemented.
        ///     </para>
        ///     <para>
        ///         If the entity is already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking the entity (rather than marking it as <see cref="EntityState.Deleted" />) since the
        ///         entity was previously added to the context and does not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="Microsoft.EntityFrameworkCore.DbContext.Attach(object)" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </remarks>
        /// <param name="entity"> The entity to remove. </param>
        /// <param name="cancellationToken"> Cancellation token. </param>
        /// <returns>
        ///     The <see cref="EntityEntry" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public virtual async Task<EntityEntry<TEntity>> RemoveAsync<TEntity>([NotNull] TEntity entity,
            CancellationToken cancellationToken = default) where TEntity : class
        {
            var result = base.Remove(entity);

            if (!(entity is ISoftDelete x)) return result;

            if (x.IsForceDelete()) return result;

            await x.LoadRelationsAsync(this, cancellationToken);

            await x.OnSoftDeleteAsync(this, cancellationToken);

            return result;
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         NOTE: This method will force delete any entity even it implements soft delete.
        ///     </para>
        ///     <para>
        ///         If the entity is already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking the entity (rather than marking it as <see cref="EntityState.Deleted" />) since the
        ///         entity was previously added to the context and does not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="Microsoft.EntityFrameworkCore.DbContext.Attach{TEntity}(TEntity)" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </remarks>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <param name="entity"> The entity to remove. </param>
        /// <returns>
        ///     The <see cref="EntityEntry{TEntity}" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public virtual EntityEntry<TEntity> ForceRemove<TEntity>([NotNull] TEntity entity) where TEntity : class
        {
            var result = base.Remove(entity);

            if (!(entity is ISoftDelete x)) return result;

            x.SetForceDelete();

            return result;
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database for not soft delete implemented models and soft delete them from database
        ///     for soft delete implemented models when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         NOTE: This method will soft delete entities that implements soft deletes and force delete entities that
        ///         soft delete not implemented.
        ///     </para>
        ///     <para>
        ///         If the entity is already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking the entity (rather than marking it as <see cref="EntityState.Deleted" />) since the
        ///         entity was previously added to the context and does not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="Microsoft.EntityFrameworkCore.DbContext.Attach(object)" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </remarks>
        /// <param name="entity"> The entity to remove. </param>
        /// <returns>
        ///     The <see cref="EntityEntry" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public override EntityEntry Remove([NotNull] object entity)
        {
            var result = base.Remove(entity);

            if (!(entity is ISoftDelete x)) return result;

            if (x.IsForceDelete()) return result;

            x.LoadRelations(this);

            x.OnSoftDelete(this);

            return result;
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database for not soft delete implemented models and soft delete them from database
        ///     for soft delete implemented models when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         NOTE: This method will soft delete entities that implements soft deletes and force delete entities that
        ///         soft delete not implemented.
        ///     </para>
        ///     <para>
        ///         If the entity is already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking the entity (rather than marking it as <see cref="EntityState.Deleted" />) since the
        ///         entity was previously added to the context and does not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="Microsoft.EntityFrameworkCore.DbContext.Attach(object)" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </remarks>
        /// <param name="entity"> The entity to remove. </param>
        /// <param name="cancellationToken"> Cancellation token. </param>
        /// <returns>
        ///     The <see cref="EntityEntry" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public virtual async Task<EntityEntry> RemoveAsync([NotNull] object entity,
            CancellationToken cancellationToken = default)
        {
            var result = base.Remove(entity);

            if (!(entity is ISoftDelete x)) return result;

            if (x.IsForceDelete()) return result;

            await x.LoadRelationsAsync(this, cancellationToken);

            await x.OnSoftDeleteAsync(this, cancellationToken);

            return result;
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         NOTE: This method will force delete any entity even it implements soft delete.
        ///     </para>
        ///     <para>
        ///         If the entity is already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking the entity (rather than marking it as <see cref="EntityState.Deleted" />) since the
        ///         entity was previously added to the context and does not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="Microsoft.EntityFrameworkCore.DbContext.Attach{TEntity}(TEntity)" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </remarks>
        /// <param name="entity"> The entity to remove. </param>
        /// <returns>
        ///     The <see cref="EntityEntry{TEntity}" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public virtual EntityEntry ForceRemove([NotNull] object entity)
        {
            var result = base.Remove(entity);

            if (!(entity is ISoftDelete x)) return result;

            x.SetForceDelete();

            return result;
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database for not soft delete implemented models and soft delete them from database
        ///     for soft delete implemented models when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         NOTE: This method will soft delete entities that implements soft deletes and force delete entities that
        ///         soft delete not implemented.
        ///     </para>
        ///     <para>
        ///         If any of the entities are already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking those entities (rather than marking them as <see cref="EntityState.Deleted" />) since those
        ///         entities were previously added to the context and do not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="Microsoft.EntityFrameworkCore.DbContext.AttachRange(object[])" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        ///     </para>
        /// </remarks>
        /// <param name="entities"> The entities to remove. </param>
        public override void RemoveRange(params object[] entities)
        {
            base.RemoveRange(entities);

            foreach (var entity in entities) {
                if (!(entity is ISoftDelete x)) continue;

                if (x.IsForceDelete()) continue;

                x.LoadRelations(this);

                x.OnSoftDelete(this);
            }
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database for not soft delete implemented models and soft delete them from database
        ///     for soft delete implemented models when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         NOTE: This method will soft delete entities that implements soft deletes and force delete entities that
        ///         soft delete not implemented.
        ///     </para>
        ///     <para>
        ///         If any of the entities are already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking those entities (rather than marking them as <see cref="EntityState.Deleted" />) since those
        ///         entities were previously added to the context and do not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="Microsoft.EntityFrameworkCore.DbContext.AttachRange(object[])" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        ///     </para>
        /// </remarks>
        /// <param name="cancellationToken"> Cancellation Token .</param>
        /// <param name="entities"> The entities to remove. </param>
        public async Task RemoveRangeAsync(CancellationToken cancellationToken = default, params object[] entities)
        {
            await RemoveRangeAsync(entities, cancellationToken);
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database for not soft delete implemented models and soft delete them from database
        ///     for soft delete implemented models when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         NOTE: This method will force delete any entity even it implements soft delete.
        ///     </para>
        ///     <para>
        ///         If any of the entities are already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking those entities (rather than marking them as <see cref="EntityState.Deleted" />) since those
        ///         entities were previously added to the context and do not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="Microsoft.EntityFrameworkCore.DbContext.AttachRange(object[])" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        ///     </para>
        /// </remarks>
        /// <param name="entities"> The entities to remove. </param>
        public void ForceRemoveRange(params object[] entities)
        {
            ForceRemoveRange((IEnumerable<object>) entities);
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database for not soft delete implemented models and soft delete them from database
        ///     for soft delete implemented models when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         NOTE: This method will soft delete entities that implements soft deletes and force delete entities that
        ///         soft delete not implemented.
        ///     </para>
        ///     <para>
        ///         If any of the entities are already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking those entities (rather than marking them as <see cref="EntityState.Deleted" />) since those
        ///         entities were previously added to the context and do not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="Microsoft.EntityFrameworkCore.DbContext.AttachRange(object[])" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        ///     </para>
        /// </remarks>
        /// <param name="entities"> The entities to remove. </param>
        public override void RemoveRange(IEnumerable<object> entities)
        {
            base.RemoveRange(entities);

            foreach (var entity in entities) {
                if (!(entity is ISoftDelete x)) continue;

                if (x.IsForceDelete()) continue;

                x.LoadRelations(this);

                x.OnSoftDelete(this);
            }
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database for not soft delete implemented models and soft delete them from database
        ///     for soft delete implemented models when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         NOTE: This method will soft delete entities that implements soft deletes and force delete entities that
        ///         soft delete not implemented.
        ///     </para>
        ///     <para>
        ///         If any of the entities are already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking those entities (rather than marking them as <see cref="EntityState.Deleted" />) since those
        ///         entities were previously added to the context and do not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="Microsoft.EntityFrameworkCore.DbContext.AttachRange(object[])" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        ///     </para>
        /// </remarks>
        /// <param name="entities"> The entities to remove. </param>
        /// <param name="cancellationToken"> Cancellation token. </param>
        public async Task RemoveRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default)
        {
            base.RemoveRange(entities);

            foreach (var entity in entities) {
                if (!(entity is ISoftDelete x)) continue;

                if (x.IsForceDelete()) continue;

                await x.LoadRelationsAsync(this, cancellationToken);

                await x.OnSoftDeleteAsync(this, cancellationToken);
            }
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database for not soft delete implemented models and soft delete them from database
        ///     for soft delete implemented models when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         NOTE: This method will force delete any entity even it implements soft delete.
        ///     </para>
        ///     <para>
        ///         If any of the entities are already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking those entities (rather than marking them as <see cref="EntityState.Deleted" />) since those
        ///         entities were previously added to the context and do not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="Microsoft.EntityFrameworkCore.DbContext.AttachRange(object[])" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()" /> is called.
        ///     </para>
        /// </remarks>
        /// <param name="entities"> The entities to remove. </param>
        public void ForceRemoveRange(IEnumerable<object> entities)
        {
            base.RemoveRange(entities);

            foreach (var entity in entities) {
                if (!(entity is ISoftDelete x)) continue;

                x.SetForceDelete();
            }
        }

        /// <summary>
        ///     Allow restoring entity from soft delete.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         NOTE: This method will restore entities that implements soft deletes by setting DeletedAt to NULL.
        ///     </para>
        ///     <para>
        ///         NOTE: After calling this method SaveChanges will be called.
        ///     </para>
        /// </remarks>
        /// <param name="entity"> The entity to restore. </param>
        /// <returns>
        ///     The <see cref="int" /> for the result of SaveChanges.
        /// </returns>
        public virtual int Restore([NotNull] ISoftDelete entity)
        {
            allowRestore = true;

            entity.DeletedAt = null;

            int result = base.SaveChanges();

            return result;
        }

        /// <summary>
        ///     Allow restoring entity from soft delete.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         NOTE: This method will restore entities that implements soft deletes by setting DeletedAt to NULL.
        ///     </para>
        ///     <para>
        ///         NOTE: After calling this method SaveChangesAsync will be called.
        ///     </para>
        /// </remarks>
        /// <param name="entity"> The entity to restore. </param>
        /// <returns>
        ///     The <see cref="int" /> for the result of SaveChangesAsync.
        /// </returns>
        public virtual async Task<int> RestoreAsync([NotNull] ISoftDelete entity)
        {
            allowRestore = true;

            entity.DeletedAt = null;

            var result = await base.SaveChangesAsync();

            return result;
        }

        /// <summary>
        ///     Allow restoring range of entities from soft delete.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         NOTE: This method will restore entities that implements soft deletes by setting DeletedAt to NULL.
        ///     </para>
        ///     <para>
        ///         NOTE: After calling this method SaveChanges will be called.
        ///     </para>
        /// </remarks>
        /// <param name="entities"> The entities to restore. </param>
        /// <returns>
        ///     The <see cref="int" /> for the result of SaveChanges.
        /// </returns>
        public virtual int RestoreRange(IEnumerable<ISoftDelete> entities)
        {
            allowRestore = true;

            foreach (var entity in entities)
            {
                entity.DeletedAt = null;
            }

            int result = base.SaveChanges();

            return result;
        }

        /// <summary>
        ///     Allow restoring range of entities from soft delete.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         NOTE: This method will restore entities that implements soft deletes by setting DeletedAt to NULL.
        ///     </para>
        ///     <para>
        ///         NOTE: After calling this method SaveChangesAsync will be called.
        ///     </para>
        /// </remarks>
        /// <param name="entities"> The entities to restore. </param>
        /// <returns>
        ///     The <see cref="int" /> for the result of SaveChangesAsync.
        /// </returns>
        public virtual async Task<int> RestoreRangeAsync(IEnumerable<ISoftDelete> entities)
        {
            allowRestore = true;

            foreach (var entity in entities)
            {
                entity.DeletedAt = null;
            }

            int result = await base.SaveChangesAsync();

            return result;
        }
    }
}