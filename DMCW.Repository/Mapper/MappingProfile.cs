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
           

            CreateMap<Product, ProductSearchResponse>();
        }
    }
}
