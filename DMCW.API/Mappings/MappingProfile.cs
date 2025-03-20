using AutoMapper;
using DMCW.Repository.Data.Entities.User;
using DMCW.ServiceInterface.Dtos.User.KlzTEch.Service.Interface.Dto;

namespace DMCW.API.Mappings
{
    using AutoMapper;
    using DMCW.Repository.Data.Entities.User;

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserServiceDto>();
            CreateMap<Address, AddressDto>();
            CreateMap<GeoCoordinates, GeoCoordinatesDto>();
            CreateMap<BusinessHours, BusinessHoursDto>();

            // Reverse mappings if needed
            CreateMap<UserServiceDto, User>();
            CreateMap<AddressDto, Address>();
            CreateMap<GeoCoordinatesDto, GeoCoordinates>();
            CreateMap<BusinessHoursDto, BusinessHours>();
        }
    }
}
