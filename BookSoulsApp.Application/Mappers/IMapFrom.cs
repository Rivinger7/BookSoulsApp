using AutoMapper;

namespace BookSoulsApp.Application.Mappers;
public interface IMapFrom<T>
{
    void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType()).ReverseMap();
}
