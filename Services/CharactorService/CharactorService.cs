using System.Collections.Generic;
using System.Linq;
using sample.Models;

namespace sample.Services.CharactorService
{
  public class CharactorService : ICharactorService
  {
    private static List<Charactor> charactors = new List<Charactor>{
      new Charactor(),
      new Charactor{Id=1, Name="Sam"}
    };
    public List<Charactor> AddCharactor(Charactor newCharactor)
    {
      charactors.Add(newCharactor);
      return charactors;
    }

    public List<Charactor> GetAllCharactors()
    {
      return charactors;
    }

    public Charactor GetCharactorById(int id)
    {
      return charactors.FirstOrDefault(c => c.Id == id);
    }
  }
}