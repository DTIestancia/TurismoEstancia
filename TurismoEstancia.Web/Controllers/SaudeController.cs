using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurismoEstancia.Domain.Data;

namespace TurismoEstancia.Web.Controllers;

/// <summary>Health-check público (monitoramento): GET /saude.</summary>
[ApiController]
[Route("saude")]
public class SaudeController : ControllerBase
{
    private readonly AppDbContext _db;

    public SaudeController(AppDbContext db) => _db = db;

    /// <summary>GET /saude — 200 quando app + banco respondem, 503 caso contrário.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        try
        {
            await _db.Database.ExecuteSqlRawAsync("SELECT 1", ct);
            return Ok(new { status = "ok", hora = DateTimeOffset.Now });
        }
        catch
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "erro" });
        }
    }
}
