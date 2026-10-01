using CodeGames.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeGames.Controllers;

// Primjer serverske provjere uloge. Skrivanje linka na klijentu nije dovoljno.
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    [HttpGet("ping")]
    public ActionResult<MessageDto> Ping()
    {
        return Ok(new MessageDto { Message = "Admin pristup je dozvoljen." });
    }
}
