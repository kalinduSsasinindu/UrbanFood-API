﻿using AutoMapper;
using DMCW.API.Dtos;
using DMCW.Repository.Data.Entities.Order;
using DMCW.Repository.Data.Entities.product;
using DMCW.Repository.Data.Entities.Search;
using DMCW.Repository.Data.Entities.User;
using DMCW.ServiceInterface.Dtos;
using DMCW.ServiceInterface.Dtos.product.DMCW.API.Dtos.Product.DMCW.API.Dtos;
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
            CreateMap<ProductReview, ProductReviewDto>();
            CreateMap<Product, ProductSearchResponse>();


        

            CreateMap<Order, OrderSearchResponse>()
             .ForMember(dest => dest.lineItemCount, opt => opt.MapFrom(src => src.LineItems.Count));

            CreateMap<Product, ProductSearchResponse>();

           

       
            CreateMap<MediaWebDto, MediaServiceDto>().ReverseMap();
            CreateMap<MediaUpdateRequestWebDto, MediaUpdateRequestServiceDto>();


        }
    }
}