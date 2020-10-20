using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using sample.Dtos.Charactor;
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
        // public async Task<IActionResult> Get()
        // {
        //   // return Ok(knight);
        //   return Ok(await _charactorService.GetAllCharactors());
        // }
        public async Task<IActionResult> Get()
        {
            // return Ok(knight);
            return Ok(await _charactorService.GetAllCharactors());
        }

        // [Route("getfirst")]
        // public async Task<IActionResult> GetFirst()
        // {
        //   return Ok((await _charactorService.GetAllCharactors())[0]);
        // }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSingle(int id) => Ok(await _charactorService.GetCharactorById(id));

        [HttpPost]
        public async Task<IActionResult> AddCharactor(AddCharactorDto newCharactor)
        {
            return Ok(await _charactorService.AddCharactor(newCharactor));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCharactor(UpdateCharactorDto updateCharactor)
        {
            ServiceResponse<GetCharactorDto> response = await _charactorService.UpdateCharactor(updateCharactor);
            if (response.Data == null)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCharactor(int id)
        {
            ServiceResponse<List<GetCharactorDto>> response = await _charactorService.DeleteCharactor(id);
            if (response.Data == null)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
    }
}