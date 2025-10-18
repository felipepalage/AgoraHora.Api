using AgoraHora.Api.Data;
using AgoraHora.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgoraHora.Api.Controllers;

[ApiController]
[Route("api/profissionais")]
public class ProfissionaisController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProfissionaisController(AppDbContext db) => _db = db;

    [HttpGet("{estabelecimentoId:int}")]
    public async Task<IActionResult> Listar(int estabelecimentoId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = _db.Profissionais.AsNoTracking()
            .Where(p => p.EstabelecimentoId == estabelecimentoId)
            .OrderBy(p => p.Nome);

        var total = await query.CountAsync();
        var dados = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new { message = "Profissionais listados.", page, pageSize, total, data = dados });
    }

    [HttpGet("id/{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        var p = await _db.Profissionais.FindAsync(id);
        if (p is null) return NotFound(new { message = "Profissional não encontrado." });
        return Ok(new { message = "Profissional encontrado.", data = p });
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] Profissional p)
    {
        if (string.IsNullOrWhiteSpace(p.Nome))
            return BadRequest(new { message = "Nome é obrigatório." });

        _db.Profissionais.Add(p);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Profissional criado.", data = p });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] Profissional p)
    {
        var dbP = await _db.Profissionais.FindAsync(id);
        if (dbP is null) return NotFound(new { message = "Profissional não encontrado." });

        dbP.Nome = p.Nome?.Trim() ?? dbP.Nome;
        dbP.Especialidade = p.Especialidade?.Trim() ?? dbP.Especialidade; // adicione aqui
        dbP.Ativo = p.Ativo;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Profissional atualizado.", data = dbP });
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var dbP = await _db.Profissionais.FindAsync(id);
        if (dbP is null) return NotFound(new { message = "Profissional não encontrado." });

        _db.Profissionais.Remove(dbP);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Profissional removido." });
    }
}
