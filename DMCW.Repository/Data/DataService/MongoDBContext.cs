﻿using DMCW.Repository.Data.Entities;
using DMCW.Repository.Data.Entities.User;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace DMCW.Repository.Data.DataService
{
    public class MongoDBContext
    {
        private readonly IMongoDatabase _database;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MongoDBContext(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            var client = new MongoClient(configuration.GetConnectionString("MongoDB"));
            _database = client.GetDatabase("dmcw-dev");
        }

        private FilteredMongoCollection<T> GetFilteredCollection<T>(string collectionName) where T : class
        {
            return new FilteredMongoCollection<T>(_database.GetCollection<T>(collectionName), _httpContextAccessor);
        }

    
        public FilteredMongoCollection<User> Users => GetFilteredCollection<User>("User");
        public FilteredMongoCollection<Sequence> Sequences => GetFilteredCollection<Sequence>("Sequence");
    }
}
