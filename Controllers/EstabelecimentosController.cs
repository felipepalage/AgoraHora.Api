using AgoraHora.Api.Data;
using AgoraHora.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgoraHora.Api.Controllers;

[ApiController]
[Route("api/estabelecimentos")]
public class EstabelecimentosController : ControllerBase
{
    private readonly AppDbContext _db;
    public EstabelecimentosController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = _db.Estabelecimentos.AsNoTracking().OrderBy(e => e.Nome);
        var total = await query.CountAsync();
        var dados = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new { message = "Estabelecimentos listados.", page, pageSize, total, data = dados });
    }

    [HttpGet("id/{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        var e = await _db.Estabelecimentos.FindAsync(id);
        if (e is null) return NotFound(new { message = "Estabelecimento não encontrado." });
        return Ok(new { message = "Estabelecimento encontrado.", data = e });
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] Estabelecimento e)
    {
        if (string.IsNullOrWhiteSpace(e.Nome))
            return BadRequest(new { message = "Nome é obrigatório." });

        _db.Estabelecimentos.Add(e);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Estabelecimento criado.", data = e });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] Estabelecimento e)
    {
        var dbE = await _db.Estabelecimentos.FindAsync(id);
        if (dbE is null) return NotFound(new { message = "Estabelecimento não encontrado." });

        dbE.Nome = e.Nome?.Trim() ?? dbE.Nome;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Estabelecimento atualizado.", data = dbE });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var dbE = await _db.Estabelecimentos.FindAsync(id);
        if (dbE is null) return NotFound(new { message = "Estabelecimento não encontrado." });

        _db.Estabelecimentos.Remove(dbE);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Estabelecimento removido." });
    }
}
