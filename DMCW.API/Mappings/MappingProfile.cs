using AutoMapper;
using DMCW.Repository.Data.Entities.User;
using DMCW.ServiceInterface.Dtos.User;

namespace DMCW.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Entity to DTO mappings
            CreateMap<User, UserServiceDto>();
            CreateMap<Address, AddressDto>();
            CreateMap<SellerProfile, SellerProfileDto>();
            CreateMap<StoreDetails, StoreDetailsDto>();
            CreateMap<SellerReview, SellerReviewDto>();

            // DTO to Entity mappings
            CreateMap<UserServiceDto, User>();
            CreateMap<AddressDto, Address>();
            CreateMap<SellerProfileDto, SellerProfile>();
            CreateMap<StoreDetailsDto, StoreDetails>();
            CreateMap<SellerReviewDto, SellerReview>();
        }
    }
}