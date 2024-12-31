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

            CreateMap<MealLogDto, MealLog>()
    .ForMember(dest => dest.TotalKcal, opt => opt.MapFrom(src => src.ProductQuantity
        .Sum(pq => pq.Product.Nutriments.EnergyKcal100g / 100 * pq.Quantity)))
    .ForMember(dest => dest.TotalCarbohydrates, opt => opt.MapFrom(src => src.ProductQuantity
        .Sum(pq => (pq.Product != null && pq.Product.Nutriments != null ? pq.Product.Nutriments.Carbohydrates100g : 0f) / 100 * pq.Quantity)))
    .ForMember(dest => dest.TotalProteins, opt => opt.MapFrom(src => src.ProductQuantity
        .Sum(pq => (pq.Product != null && pq.Product.Nutriments != null ? pq.Product.Nutriments.Proteins100g : 0f) / 100 * pq.Quantity)))
    .ForMember(dest => dest.TotalFat, opt => opt.MapFrom(src => src.ProductQuantity
        .Sum(pq => (pq.Product != null && pq.Product.Nutriments != null ? pq.Product.Nutriments.Fat100g : 0f) / 100 * pq.Quantity)))    
     .ForMember(dest => dest.TotalFat, opt => opt.MapFrom(src => src.ProductQuantity
        .Sum(pq => (pq.Product != null && pq.Product.Nutriments != null ? pq.Product.Nutriments.Fiber100g : 0f) / 100 * pq.Quantity)));
                

        }
        

    }
}
