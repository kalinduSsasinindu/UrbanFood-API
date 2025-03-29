using AutoMapper;
using DMCW.Repository.Data.DataService;
using DMCW.ServiceInterface.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.Service.Services
{
    public class ProductService : IproductService
    {
        private readonly MongoDBContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        public ProductService(MongoDBContext mongoDBContext,IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IMapper mapper) {
            _context = mongoDBContext;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _mapper = mapper;

        }
       
    }
}
