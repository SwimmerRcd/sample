using System.Collections.Generic;
using sample.Models;

namespace sample.Services.CharactorService
{
  public interface ICharactorService
  {
    List<Charactor> GetAllCharactors();
    Charactor GetCharactorById(int id);
    List<Charactor> AddCharactor(Charactor newCharactor);
  }
}