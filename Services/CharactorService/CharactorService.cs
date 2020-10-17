using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using sample.Dtos.Charactor;
using sample.Models;

namespace sample.Services.CharactorService
{
  public class CharactorService : ICharactorService
  {
    private readonly IMapper _mapper;
    public CharactorService(IMapper mapper)
    {
      _mapper = mapper;
    }
    private static List<Charactor> charactors = new List<Charactor>{
      new Charactor(),
      new Charactor{Id=1, Name="Sam"}
    };
    public async Task<ServiceResponse<List<GetCharactorDto>>> AddCharactor(AddCharactorDto newCharactor)
    {
      ServiceResponse<List<GetCharactorDto>> serviceResponse = new ServiceResponse<List<GetCharactorDto>>();
      Charactor charactor = _mapper.Map<Charactor>(newCharactor);
      charactor.Id = charactors.Max(c => c.Id) + 1;
      charactors.Add(charactor);
      serviceResponse.Data = (charactors.Select(c => _mapper.Map<GetCharactorDto>(c))).ToList();
      return serviceResponse;
    }

    public async Task<ServiceResponse<List<GetCharactorDto>>> GetAllCharactors()
    {
      ServiceResponse<List<GetCharactorDto>> serviceResponse = new ServiceResponse<List<GetCharactorDto>>();
      serviceResponse.Data = (charactors.Select(c => _mapper.Map<GetCharactorDto>(c))).ToList();
      return serviceResponse;
    }

    public async Task<ServiceResponse<GetCharactorDto>> GetCharactorById(int id)
    {
      ServiceResponse<GetCharactorDto> serviceResponse = new ServiceResponse<GetCharactorDto>();
      serviceResponse.Data = _mapper.Map<GetCharactorDto>(charactors.FirstOrDefault(c => c.Id == id));
      return serviceResponse;
    }

    public async Task<ServiceResponse<GetCharactorDto>> UpdateCharactor(UpdateCharactorDto updateCharactorDto)
    {
      ServiceResponse<GetCharactorDto> serviceResponse = new ServiceResponse<GetCharactorDto>();
      try
      {
        Charactor charactor = charactors.FirstOrDefault(c => c.Id == updateCharactorDto.Id);
        charactor.Name = updateCharactorDto.Name;
        charactor.HitPoints = updateCharactorDto.HitPoints;
        charactor.Strength = updateCharactorDto.Strength;
        charactor.Defense = updateCharactorDto.Defense;
        charactor.Intelligence = updateCharactorDto.Intelligence;
        charactor.Class = updateCharactorDto.Class;
        serviceResponse.Data = _mapper.Map<GetCharactorDto>(charactor);
      }
      catch (Exception ex)
      {
        serviceResponse.Success = false;
        serviceResponse.Message = ex.Message;
      }
      return serviceResponse;
    }

    public async Task<ServiceResponse<List<GetCharactorDto>>> DeleteCharactor(int id)
    {
      ServiceResponse<List<GetCharactorDto>> serviceResponse = new ServiceResponse<List<GetCharactorDto>>();
      try
      {
        Charactor charactor = charactors.First(c => c.Id == id);
        charactors.Remove(charactor);
        serviceResponse.Data = (charactors.Select(c => _mapper.Map<GetCharactorDto>(c))).ToList();
      }
      catch (Exception ex)
      {
        serviceResponse.Success = false;
        serviceResponse.Message = ex.Message;
      }
      return serviceResponse;
    }
  }
}