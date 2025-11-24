using AutoMapper;
using WordMaster.Application.Responses.Favorite;
using WordMaster.Application.Responses.Unknows;
using WordMaster.Application.Responses.Word;
using WordMaster.Domain.Entities;

namespace WordMaster.Application.Mapping;

public class MapProfile : Profile
{
    public MapProfile()
    {
        CreateMap<Word, WordResponse>().ReverseMap();
        CreateMap<Favorite, FavoriteResponse>().ReverseMap();
        CreateMap<Unknows, UnknowsResponse>().ReverseMap();
    }
}
