using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using sample.Models;
using sample.Services.CharactorService;

namespace sample.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class CharactorController : ControllerBase
  {
    private readonly ICharactorService _charactorService;

    // private static Charactor knight = new Charactor();
    public CharactorController(ICharactorService charactorService)
    {
      this._charactorService = charactorService;
    }

    [Route("GetAll")]
    public IActionResult Get()
    {
      // return Ok(knight);
      return Ok(_charactorService.GetAllCharactors());
    }

    [Route("getfirst")]
    public IActionResult GetFirst()
    {
      return Ok(_charactorService.GetAllCharactors()[0]);
    }

    [HttpGet("{id}")]
    public IActionResult GetSingle(int id) => Ok(_charactorService.GetCharactorById(id));

    [HttpPost]
    public IActionResult AddCharactor(Charactor newCharactor)
    {
      return Ok(_charactorService.AddCharactor(newCharactor));
    }
  }
}