using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace DMCW.API.Extensions
{
    public static class ServiceRegistrationExtensions
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            /*services.AddSingleton<BlobService>();
            services.AddScoped<IBusinessUnitService, BusinessUnitService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRefService, RefService>();
            services.AddScoped<IPrintService, PrintService>();
            services.AddScoped<ITrackingService, TrackingService>();
            services.AddScoped<IUtilityService, UtilityService>();
            services.AddScoped<AuthorizationContext>();
            services.AddScoped<MongoDBContext>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddSingleton<INotificationService, NotificationService>();

            services.AddScoped<FardarDomesticsService>();
            services.AddScoped<FDEDomesticService>();
            services.AddScoped<CityPakService>();
            services.AddScoped<ICourierServiceFactory, CourierServiceFactory>();
            services.AddScoped<ITagsService, TagsService>();
            services.AddScoped<IValidator<Customer>, CustomerValidator>();
            services.AddScoped<ICollectionService, CollectionService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<INotificationBuilder, NotificationBuilder>();
            services.AddMemoryCache();
            services.AddScoped<IHostingService, HostingService>();*/
        }
    }
}
