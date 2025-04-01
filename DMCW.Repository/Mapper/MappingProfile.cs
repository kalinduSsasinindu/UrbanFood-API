using AutoMapper;
using DMCW.Repository.Data.Entities.product;
using DMCW.Repository.Data.Entities.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.Repository.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductSearchResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ImgUrls, opt => opt.MapFrom(src => src.ImgUrls))
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src.Variants))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src =>
                    src.Variants != null && src.Variants.Count > 0
                        ? (src.Variants[0].Price.HasValue ? src.Variants[0].Price.Value : 0)
                        : 0));
        }
    }
}
