using AutoMapper;
using WordMaster.Application.Dto;
using WordMaster.Domain.Entities;

namespace WordMaster.Application.Mapping;

public class MapProfile : Profile
{
    public MapProfile()
    {
        CreateMap<Word, WordDto>().ReverseMap();
        CreateMap<Favorite, FavoriteDto>().ReverseMap();
        CreateMap<Unknows, UnknowsDto>().ReverseMap();
    }
}
