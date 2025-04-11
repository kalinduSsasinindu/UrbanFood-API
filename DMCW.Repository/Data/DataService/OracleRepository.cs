using DMCW.Repository.Data.Entities.Base;
using Microsoft.AspNetCore.Http;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Reflection;
using System.Text;

namespace DMCW.Repository.Data.DataService
{
    public class OracleRepository<T> where T : class
    {
        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string _clientId => Helper.Utility.GetUserIdFromClaims(_httpContextAccessor);

        public OracleRepository(string connectionString, string tableName, IHttpContextAccessor httpContextAccessor)
        {
            _connectionString = connectionString;
            _tableName = tableName;
            _httpContextAccessor = httpContextAccessor;
        }

        private OracleConnection CreateConnection()
        {
            var connection = new OracleConnection(_connectionString);
            return connection;
        }

        private string ApplyUserFilter(string whereClause = "")
        {
            if (typeof(IUserOwnedEntity).IsAssignableFrom(typeof(T)) && !string.IsNullOrEmpty(_clientId))
            {
                if (string.IsNullOrEmpty(whereClause))
                {
                    return $" WHERE CLIENT_ID = :clientId AND IS_DELETED = 0";
                }
                else
                {
                    return $" {whereClause} AND CLIENT_ID = :clientId AND IS_DELETED = 0";
                }
            }
            
            if (string.IsNullOrEmpty(whereClause))
            {
                return " WHERE IS_DELETED = 0";
            }
            else
            {
                return $" {whereClause} AND IS_DELETED = 0";
            }
        }

        private void AddClientIdParameter(OracleCommand command)
        {
            if (typeof(IUserOwnedEntity).IsAssignableFrom(typeof(T)) && !string.IsNullOrEmpty(_clientId))
            {
                command.Parameters.Add(new OracleParameter("clientId", _clientId));
            }
        }

        // Find methods
        public async Task<T> FindOneAsync(string whereClause = "", Dictionary<string, object> parameters = null)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var filteredWhereClause = ApplyUserFilter(whereClause);
            var sql = $"SELECT * FROM {_tableName}{filteredWhereClause} FETCH FIRST 1 ROW ONLY";

            using var command = new OracleCommand(sql, connection);
            AddClientIdParameter(command);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.Add(new OracleParameter(param.Key, param.Value));
                }
            }

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapToObject(reader);
            }

            return null;
        }

        public async Task<List<T>> FindAsync(string whereClause = "", Dictionary<string, object> parameters = null)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var filteredWhereClause = ApplyUserFilter(whereClause);
            var sql = $"SELECT * FROM {_tableName}{filteredWhereClause}";

            using var command = new OracleCommand(sql, connection);
            AddClientIdParameter(command);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.Add(new OracleParameter(param.Key, param.Value));
                }
            }

            var results = new List<T>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(MapToObject(reader));
            }

            return results;
        }

        public async Task<List<T>> FindWithPaginationAsync(int skip, int limit, string orderByColumn = "CREATED_AT", bool descending = true, string whereClause = "", Dictionary<string, object> parameters = null)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var filteredWhereClause = ApplyUserFilter(whereClause);
            var sortDirection = descending ? "DESC" : "ASC";
            
            var sql = @$"
                SELECT * FROM (
                    SELECT t.*, ROW_NUMBER() OVER (ORDER BY {orderByColumn} {sortDirection}) rn 
                    FROM {_tableName} t
                    {filteredWhereClause}
                ) 
                WHERE rn > :skip AND rn <= :limit + :skip";

            using var command = new OracleCommand(sql, connection);
            AddClientIdParameter(command);
            command.Parameters.Add(new OracleParameter("skip", skip));
            command.Parameters.Add(new OracleParameter("limit", limit));

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.Add(new OracleParameter(param.Key, param.Value));
                }
            }

            var results = new List<T>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(MapToObject(reader));
            }

            return results;
        }

        public async Task<long> CountAsync(string whereClause = "", Dictionary<string, object> parameters = null)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var filteredWhereClause = ApplyUserFilter(whereClause);
            var sql = $"SELECT COUNT(*) FROM {_tableName}{filteredWhereClause}";

            using var command = new OracleCommand(sql, connection);
            AddClientIdParameter(command);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.Add(new OracleParameter(param.Key, param.Value));
                }
            }

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt64(result);
        }

        // Insert methods
        public async Task<string> InsertOneAsync(T entity)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            // Set ClientId for user-owned entities
            if (!string.IsNullOrEmpty(_clientId) && entity is IUserOwnedEntity userOwnedEntity)
            {
                userOwnedEntity.ClientId = _clientId;
            }

            // Set created time for BaseEntity
            if (entity is BaseEntity baseEntity)
            {
                if (string.IsNullOrEmpty(baseEntity.Id))
                {
                    baseEntity.Id = Guid.NewGuid().ToString();
                }
                baseEntity.CreatedAt = DateTime.UtcNow;
                baseEntity.UpdatedAt = DateTime.UtcNow;
                baseEntity.IsDeleted = false;
            }

            var properties = typeof(T).GetProperties()
                .Where(p => p.CanRead && p.CanWrite)
                .ToList();

            var columnNames = string.Join(", ", properties.Select(p => p.Name.ToUpper()));
            var parameterNames = string.Join(", ", properties.Select(p => $":{p.Name}"));

            var sql = $"INSERT INTO {_tableName} ({columnNames}) VALUES ({parameterNames})";

            using var command = new OracleCommand(sql, connection);
            foreach (var prop in properties)
            {
                var value = prop.GetValue(entity);
                command.Parameters.Add(new OracleParameter(prop.Name, value ?? DBNull.Value));
            }

            await command.ExecuteNonQueryAsync();

            // Return the ID of the inserted entity
            if (entity is BaseEntity baseEntity2)
            {
                return baseEntity2.Id;
            }
            return null;
        }

        public async Task InsertManyAsync(IEnumerable<T> entities)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var entity in entities)
                {
                    // Set ClientId for user-owned entities
                    if (!string.IsNullOrEmpty(_clientId) && entity is IUserOwnedEntity userOwnedEntity)
                    {
                        userOwnedEntity.ClientId = _clientId;
                    }

                    // Set created time for BaseEntity
                    if (entity is BaseEntity baseEntity)
                    {
                        if (string.IsNullOrEmpty(baseEntity.Id))
                        {
                            baseEntity.Id = Guid.NewGuid().ToString();
                        }
                        baseEntity.CreatedAt = DateTime.UtcNow;
                        baseEntity.UpdatedAt = DateTime.UtcNow;
                        baseEntity.IsDeleted = false;
                    }

                    var properties = typeof(T).GetProperties()
                        .Where(p => p.CanRead && p.CanWrite)
                        .ToList();

                    var columnNames = string.Join(", ", properties.Select(p => p.Name.ToUpper()));
                    var parameterNames = string.Join(", ", properties.Select(p => $":{p.Name}"));

                    var sql = $"INSERT INTO {_tableName} ({columnNames}) VALUES ({parameterNames})";

                    using var command = new OracleCommand(sql, connection);
                    command.Transaction = transaction;

                    foreach (var prop in properties)
                    {
                        var value = prop.GetValue(entity);
                        command.Parameters.Add(new OracleParameter(prop.Name, value ?? DBNull.Value));
                    }

                    await command.ExecuteNonQueryAsync();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Update methods
        public async Task<bool> UpdateOneAsync(string id, Dictionary<string, object> updates)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            // Include UpdatedAt in updates
            updates["UpdatedAt"] = DateTime.UtcNow;

            var setClause = new StringBuilder();
            foreach (var update in updates)
            {
                if (setClause.Length > 0)
                    setClause.Append(", ");
                setClause.Append($"{update.Key.ToUpper()} = :{update.Key}");
            }

            var whereClause = ApplyUserFilter(" WHERE ID = :id");
            var sql = $"UPDATE {_tableName} SET {setClause}{whereClause}";

            using var command = new OracleCommand(sql, connection);
            AddClientIdParameter(command);
            command.Parameters.Add(new OracleParameter("id", id));

            foreach (var update in updates)
            {
                command.Parameters.Add(new OracleParameter(update.Key, update.Value ?? DBNull.Value));
            }

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> ReplaceOneAsync(string id, T replacement)
        {
            // First soft delete the existing entity
            var deleted = await SoftDeleteOneAsync(id);
            if (!deleted)
                return false;

            // Then insert the replacement
            if (replacement is BaseEntity baseEntity)
            {
                baseEntity.Id = id;
            }

            await InsertOneAsync(replacement);
            return true;
        }

        // Delete methods
        public async Task<bool> DeleteOneAsync(string id)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var whereClause = ApplyUserFilter(" WHERE ID = :id");
            var sql = $"DELETE FROM {_tableName}{whereClause}";

            using var command = new OracleCommand(sql, connection);
            AddClientIdParameter(command);
            command.Parameters.Add(new OracleParameter("id", id));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> SoftDeleteOneAsync(string id)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var whereClause = ApplyUserFilter(" WHERE ID = :id");
            var sql = $"UPDATE {_tableName} SET IS_DELETED = 1, DELETED_AT = :deletedAt{whereClause}";

            using var command = new OracleCommand(sql, connection);
            AddClientIdParameter(command);
            command.Parameters.Add(new OracleParameter("id", id));
            command.Parameters.Add(new OracleParameter("deletedAt", DateTime.UtcNow));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // Helper methods
        private T MapToObject(System.Data.Common.DbDataReader reader)
        {
            var entity = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var property = properties.FirstOrDefault(p => 
                    p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                if (property != null && property.CanWrite)
                {
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    if (value != null)
                    {
                        // Handle type conversion
                        if (property.PropertyType == typeof(string))
                        {
                            property.SetValue(entity, value.ToString());
                        }
                        else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                        {
                            property.SetValue(entity, Convert.ToInt32(value));
                        }
                        else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
                        {
                            property.SetValue(entity, Convert.ToDecimal(value));
                        }
                        else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                        {
                            property.SetValue(entity, Convert.ToDateTime(value));
                        }
                        else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
                        {
                            property.SetValue(entity, Convert.ToBoolean(value));
                        }
                        else
                        {
                            property.SetValue(entity, value);
                        }
                    }
                }
            }

            return entity;
        }

        // Direct SQL execution
        public async Task<DataTable> ExecuteQueryAsync(string sql, params OracleParameter[] parameters)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new OracleCommand(sql, connection);
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            var dataTable = new DataTable();
            using var adapter = new OracleDataAdapter(command);
            adapter.Fill(dataTable);

            return dataTable;
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, params OracleParameter[] parameters)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new OracleCommand(sql, connection);
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteNonQueryAsync();
        }

        public async Task<object> ExecuteScalarAsync(string sql, params OracleParameter[] parameters)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new OracleCommand(sql, connection);
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteScalarAsync();
        }
    }
} 