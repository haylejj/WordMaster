using AutoMapper;
using Core.Dto;
using Core.Entity;

namespace Service.Mapping;

public class MapProfile : Profile
{
    public MapProfile()
    {
        CreateMap<Word, WordDto>().ReverseMap();
        CreateMap<Favorite, FavoriteDto>().ReverseMap();
        CreateMap<Unknows, UnknowsDto>().ReverseMap();
    }
}
