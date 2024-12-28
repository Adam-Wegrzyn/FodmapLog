using AutoMapper;
using Data.Common.DTO;
using DataAccess.Entities;

namespace Core
{
    public class mapperConfig: Profile
    {
        public mapperConfig()
        {
            CreateMap<MealLogDto, MealLog>().ReverseMap();
            CreateMap<ProductDto, Product>().ReverseMap();
            CreateMap<ProductQuantityDto, ProductQuantity>().ReverseMap();
            CreateMap<NutrimentsDto, Nutriments>().ReverseMap();

            CreateMap<MealLog, MealLogDto>()
                .ForMember(dest => dest.TotalKcal, opt => opt.MapFrom(src => src.ProductQuantity
                .Sum(pq => pq.Product.Nutriments.EnergyKcal100g / 100 * pq.Quantity)));
        }
        

    }
}
