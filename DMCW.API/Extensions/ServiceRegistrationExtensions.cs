using DMCW.Repository.Data.DataService;
using DMCW.Service.Services;
using DMCW.Service.Services.blob;
using DMCW.Service.Services.Tags;
using DMCW.ServiceInterface.Interfaces;

namespace DMCW.API.Extensions
{
    public static class ServiceRegistrationExtensions
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            // Database contexts
            services.AddScoped<MongoDBContext>();
            services.AddScoped<OracleDBContext>();
            
            // Services
            services.AddSingleton<CloudinaryService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ITagsService, TagsService>();
            services.AddScoped<IOrderService, OrderService>();
            
            // Dual database services
            services.AddScoped<DualDatabaseService>();
        }
    }
}
