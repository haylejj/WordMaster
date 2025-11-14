using AutoMapper;
using WordMaster.Application.Dto.Favorite;
using WordMaster.Application.Dto.Unknows;
using WordMaster.Application.Dto.Word;
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
