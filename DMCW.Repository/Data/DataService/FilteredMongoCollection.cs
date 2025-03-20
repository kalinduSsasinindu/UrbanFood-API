using DMCW.Repository.Data.Entities.Base;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;


namespace DMCW.Repository.Data.DataService
{
    public class FilteredMongoCollection<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string _clientId => Helper.Utility.GetUserIdFromClaims(_httpContextAccessor);

        public FilteredMongoCollection(IMongoCollection<T> collection, IHttpContextAccessor httpContextAccessor)
        {
            _collection = collection;
            _httpContextAccessor = httpContextAccessor;
        }

        private FilterDefinition<T> ApplyUserFilter(FilterDefinition<T> filter)
        {

            if (typeof(IUserOwnedEntity).IsAssignableFrom(typeof(T)))
            {
                var userFilter = Builders<T>.Filter.Eq("ClientId", _clientId);
                return filter & userFilter;
            }
            else
            {
                return filter;
            }
        }

        public async Task<T> FindOneAsync(FilterDefinition<T> filter)
        {
            var filtered = ApplyUserFilter(filter);
            return await _collection.Find(filtered).FirstOrDefaultAsync();
        }

        public async Task<long> CountDocumentsAsync(FilterDefinition<T> filter)
        {
            var filtered = ApplyUserFilter(filter);
            return await _collection.CountDocumentsAsync(filtered);
        }

        public async Task<List<T>> FindAsync(FilterDefinition<T> filter)
        {
            var filtered = ApplyUserFilter(filter);
            return await _collection.Find(filtered).ToListAsync();
        }

        public IFindFluent<T, T> Find(FilterDefinition<T> filter)
        {
            var filtered = ApplyUserFilter(filter);
            var combinedFilter = Builders<T>.Filter.And(filtered, Builders<T>.Filter.Eq("isDeleted", false));

            return _collection.Find(combinedFilter);
        }
        public IAggregateFluent<T> Aggregate()
        {
            return _collection.Aggregate();
        }
        public async Task InsertOneAsync(T document)
        {
            if (!string.IsNullOrEmpty(_clientId))
            {
                if (document is IUserOwnedEntity userOwnedEntity)
                {
                    userOwnedEntity.ClientId = _clientId;
                }
            }
            await _collection.InsertOneAsync(document);
        }

        public async Task<UpdateResult> UpdateOneAsync(FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            var filtered = ApplyUserFilter(filter);
            return await _collection.UpdateOneAsync(filtered, update);
        }
        public async Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<T> filter, T replacement)
        {
            var filtered = ApplyUserFilter(filter);
            return await _collection.ReplaceOneAsync(filtered, replacement);
        }

        public async Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<T> filter, T replacement, ReplaceOptions options)
        {
            var filtered = ApplyUserFilter(filter);
            return await _collection.ReplaceOneAsync(filtered, replacement, options);
        }

        public async Task<DeleteResult> DeleteOneAsync(FilterDefinition<T> filter)
        {
            var filtered = ApplyUserFilter(filter);
            return await _collection.DeleteOneAsync(filtered);
        }

        public async Task<UpdateResult> SoftDeleteOneAsync(FilterDefinition<T> filter)
        {
            var filtered = ApplyUserFilter(filter);

            // Define an update operation to set a "deleted" flag or similar
            var updateDefinition = Builders<T>.Update.Set("IsDeleted", true)
                                                     .Set("DeletedAt", DateTime.UtcNow);

            // Perform the update operation
            return await _collection.UpdateOneAsync(filtered, updateDefinition);
        }
        public async Task<T> FindOneAndUpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T> options)
        {
            var filtered = ApplyUserFilter(filter);
            return await _collection.FindOneAndUpdateAsync(filtered, update, options);
        }

        public async Task InsertManyAsync(IEnumerable<T> documents)
        {
            // Update each document with the client ID if necessary
            if (!string.IsNullOrEmpty(_clientId))
            {
                foreach (var document in documents)
                {
                    if (document is IUserOwnedEntity userOwnedEntity)
                    {
                        userOwnedEntity.ClientId = _clientId;
                    }
                }
            }

            // Insert all documents into the collection
            await _collection.InsertManyAsync(documents);
        }
        public async Task<T> FindByEmailAsync(string email)
        {
            // Only filter by email and IsDeleted, ignore ClientId
            var filter = Builders<T>.Filter.And(
                Builders<T>.Filter.Eq("Email", email),
                Builders<T>.Filter.Eq("IsDeleted", false)
            );
            // Use _collection directly to bypass the ClientId filter
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

    }
}
