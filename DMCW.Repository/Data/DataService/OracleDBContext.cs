using DMCW.Repository.Data.Entities;
using DMCW.Repository.Data.Entities.Order;
using DMCW.Repository.Data.Entities.product;
using DMCW.Repository.Data.Entities.User;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Tag = DMCW.Repository.Data.Entities.Tags.Tag;

namespace DMCW.Repository.Data.DataService
{
    public class OracleDBContext
    {
        private readonly string _connectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string _clientId => Helper.Utility.GetUserIdFromClaims(_httpContextAccessor);

        public OracleDBContext(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _connectionString = configuration.GetConnectionString("OracleDB");
        }

        public OracleRepository<Product> Products => GetOracleRepository<Product>("PRODUCTS");
        public OracleRepository<User> Users => GetOracleRepository<User>("USERS");
        public OracleRepository<Tag> Tags => GetOracleRepository<Tag>("TAGS");
        public OracleRepository<Sequence> Sequences => GetOracleRepository<Sequence>("SEQUENCES");
        public OracleRepository<Order> Orders => GetOracleRepository<Order>("ORDERS");

        private OracleRepository<T> GetOracleRepository<T>(string tableName) where T : class
        {
            return new OracleRepository<T>(_connectionString, tableName, _httpContextAccessor);
        }

        // Direct database access methods
        public OracleConnection CreateConnection()
        {
            var connection = new OracleConnection(_connectionString);
            return connection;
        }

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