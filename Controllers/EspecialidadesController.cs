using AgoraHora.Api.Data;
using AgoraHora.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgoraHora.Api.Controllers;

[ApiController]
[Route("api/especialidades")]
public sealed class EspecialidadesController : ControllerBase
{
    private readonly AppDbContext _db;
    public EspecialidadesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? q = null)
    {
        var list = _db.Especialidades.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
            return Ok(await list.Where(x => x.Nome.Contains(q)).OrderBy(x => x.Nome).ToListAsync());
        return Ok(await list.OrderBy(x => x.Nome).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] Especialidade esp)
    {
        if (string.IsNullOrWhiteSpace(esp.Nome)) return BadRequest(new { message = "Nome é obrigatório." });
        esp.Nome = esp.Nome.Trim();
        var exists = await _db.Especialidades.AnyAsync(x => x.Nome == esp.Nome);
        if (exists) return Conflict(new { message = "Já existe." });

        _db.Especialidades.Add(esp);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Criada.", data = esp });
    }
}
