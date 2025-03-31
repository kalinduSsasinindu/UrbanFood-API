

using DMCW.Repository.Data.Entities.Tags;

namespace DMCW.ServiceInterface.Interfaces
{
    public interface ITagsService
    {
        Task<Tag> AddOrUpdateTagAsync(string name, string kind);
        Task<Tag> GetTagAsync(string name, string kind);
        Task<List<Tag>> GetTagsByClientAsync(string clientId);
        Task DeleteTagAsync(string tagId);
        Task<List<Tag>> GetTagsByKindAsync(string kind);

    }
}
