# Ef Core Soft Delete implementation
A very simple implementation for soft deleting entities in 
[Ef Core](https://github.com/dotnet/efcore) library.   
## Usage:
#### Step 1:
Extend your application db context from `SoftDeletes.Core.DbContext`.
#### Step 2:
In entities you want to add soft delete support, implement `SoftDeletes.ModelTools.ISoftDelete` 
interface.   
It will a column named `DeletedAt` to your entity so you need to add a migration and update the database tables.
#### Step 3:
Load relations you want to delete in soft deleting the entity in `LoadRelationsAsync` and `LoadRelations` methods.
#### Step 4:
Delete relations you want to delete in soft deleting the entity in `OnSoftDeleteAsync` and `OnSoftDelete` methods.
#### Step 5:
For soft deleting an entity use `Remove` and `RemoveAsync` methods.  These methods will 
**soft delete** an `ISoftDelete` implemented entity and **force delete** an not implemented entity.   
For force deleting **any** entity use `ForceRemove` method.
#### Important Note: 
It doesn't support `DbSet` yet, so you have to use `SoftDeletes.Core.DbContext` methods for removing 
instead of using `DbSet` methods.
### Extra:
##### `ITimestamps` interface:
An interface for saving `CreatedAt` and `UpdatedAt` date and time of entities.   
You can implement it in your entities. That will add to columns named 
`CreatedAt` and `UpdatedAt` to your entities.
##### `ModelExtenstion` abstract class:
An abstract class that implements `ITimestamps` and `ISoftDelete` interfaces.   
You can use it in your entities.

### Demo:
[sample project](https://github.com/AshkanAbd/efCoreSoftDeletesSample) for this implementation.

### Donation:
If you like it, you can donate BTC(bitcoin) to this wallet `1LhGtAqbCt5Wra55vmNoiu3Cbzrsnye3pk`

### Proposal:
[The proposal](https://1drv.ms/b/s!AirwjkMOI-BwkAzedA6E6YVkZqjQ?e=vfV2hq) for this implementation.

