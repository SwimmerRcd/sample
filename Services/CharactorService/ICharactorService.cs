using System.Collections.Generic;
using System.Threading.Tasks;
using sample.Dtos.Charactor;
using sample.Models;

namespace sample.Services.CharactorService
{
  public interface ICharactorService
  {
    // Task<ServiceResponse<List<GetCharactorDto>>> GetAllCharactors();
    Task<ServiceResponse<List<GetCharactorDto>>> GetAllCharactors();
    Task<ServiceResponse<GetCharactorDto>> GetCharactorById(int id);
    Task<ServiceResponse<List<GetCharactorDto>>> AddCharactor(AddCharactorDto newCharactor);
    Task<ServiceResponse<GetCharactorDto>> UpdateCharactor(UpdateCharactorDto updateCharactorDto);
    Task<ServiceResponse<List<GetCharactorDto>>> DeleteCharactor(int id);
  }
}