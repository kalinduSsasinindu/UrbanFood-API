using DMCW.Repository.Data.DataService;
using DMCW.ServiceInterface.Interfaces;
using MongoDB.Driver;
using Tag = DMCW.Repository.Data.Entities.Tags.Tag;

namespace DMCW.Service.Services.Tags
{
    public class TagsService : ITagsService
    {
        private readonly MongoDBContext _context;

        public TagsService(MongoDBContext context)
        {
            _context = context;
        }

        public async Task<Tag> AddOrUpdateTagAsync(string name, string kind)
        {
            var filter = Builders<Tag>.Filter.And(
                Builders<Tag>.Filter.Eq(t => t.Name, name),
                Builders<Tag>.Filter.Eq(t => t.Kind, kind)
            );

            var existingTag = await _context.Tags.Find(filter).FirstOrDefaultAsync();

            if (existingTag != null)
            {
                return existingTag;
            }

            var newTag = new Tag
            {
                Name = name,
                Kind = kind,
            };

            await _context.Tags.InsertOneAsync(newTag);

            return newTag;
        }
        public async Task<Tag> GetTagAsync(string name, string kind)
        {
            var filter = Builders<Tag>.Filter.And(
                Builders<Tag>.Filter.Eq(t => t.Name, name),
                Builders<Tag>.Filter.Eq(t => t.Kind, kind)
            );

            return await _context.Tags.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<Tag>> GetTagsByClientAsync(string clientId)
        {
            var filter = Builders<Tag>.Filter.Eq(t => t.ClientId, clientId);
            return await _context.Tags.Find(filter).ToListAsync();
        }

        public async Task DeleteTagAsync(string tagId)
        {
            var filter = Builders<Tag>.Filter.Eq(t => t.Id, tagId);
            await _context.Tags.DeleteOneAsync(filter);
        }

        public async Task<List<Tag>> GetTagsByKindAsync(string kind)
        {
            var filter = Builders<Tag>.Filter.Eq(t => t.Kind, kind);
            return await _context.Tags.Find(filter).ToListAsync();
        }
    }
}
