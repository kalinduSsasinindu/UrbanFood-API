using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.ServiceInterface.Dtos.User
{
    using System;
    using System.Collections.Generic;

    namespace KlzTEch.Service.Interface.Dto
    {
        public class UserServiceDto
        {
            public string Id { get; set; }
            public string GoogleId { get; set; }
            public string ClientId { get; set; }
            public string Email { get; set; }
            public string Name { get; set; }
            public string Phone { get; set; }
            public string ProfilePictureUrl { get; set; }
            public string UserRole { get; set; }
            public AddressDto Address { get; set; }
            public string StoreName { get; set; }
            public string StoreDescription { get; set; }
            public Dictionary<string, BusinessHoursDto> BusinessHours { get; set; }
            public bool? IsVerifiedSeller { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public DateTime? LastLoginDate { get; set; }
            public bool IsEmailVerified { get; set; }
            public bool IsActive { get; set; }
        }

        public class AddressDto
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string ZipCode { get; set; }
            public GeoCoordinatesDto Coordinates { get; set; }
        }

        public class GeoCoordinatesDto
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        public class BusinessHoursDto
        {
            public string OpenTime { get; set; }
            public string CloseTime { get; set; }
            public bool IsClosed { get; set; }
        }

        public class UserProfileUpdateDto
        {
            public string Name { get; set; }
            public string Phone { get; set; }
            public AddressDto Address { get; set; }
        }

        public class SellerProfileUpdateDto
        {
            public string StoreName { get; set; }
            public string StoreDescription { get; set; }
            public Dictionary<string, BusinessHoursDto> BusinessHours { get; set; }
        }
    }
}
