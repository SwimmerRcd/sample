using AutoMapper;
using sample.Dtos.Charactor;
using sample.Models;

namespace sample
{
  public class AutoMapperProfile : Profile
  {
    public AutoMapperProfile()
    {
      CreateMap<Charactor, GetCharactorDto>();
      CreateMap<AddCharactorDto, Charactor>();
    }
  }
}