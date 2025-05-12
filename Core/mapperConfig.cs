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
            CreateMap<SymptomsLog, SymptomsLogDto>().ReverseMap();
            CreateMap<Symptom, SymptomDto>().ReverseMap();
      

        }
        

    }
}
