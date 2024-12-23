using AutoMapper;
using Data.Common.DTO;
using DataAccess.Entities;

namespace Core
{
    public class mapperConfig: Profile
    {
        public mapperConfig()
        {
            CreateMap<MealDto, Meal>().ReverseMap();
            CreateMap<ProductDto, Product>().ReverseMap();
            CreateMap<ProductQuantityDto, ProductQuantity>().ReverseMap();
            CreateMap<QuantityDto, Quantity>().ReverseMap();

            CreateMap<Meal, MealDto>()
                .ForMember(dest => dest.TotalKcal, opt => opt.MapFrom(src => src.ProductQuantity
                .Sum(pq => pq.Product.Nutriments.EnergyKcal100g / 100 * pq.Quantity.Amount)));
        }
        

    }
}
