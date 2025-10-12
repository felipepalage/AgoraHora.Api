using AgoraHora.Api.Data;
using AgoraHora.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgoraHora.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ClientesController(AppDbContext db) => _db = db;

    [HttpGet("{estabelecimentoId:int}")]
    public async Task<IActionResult> Listar(int estabelecimentoId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = _db.Clientes.AsNoTracking()
            .Where(c => c.EstabelecimentoId == estabelecimentoId)
            .OrderBy(c => c.Nome);

        var total = await query.CountAsync();
        var dados = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new { message = "Clientes listados.", page, pageSize, total, data = dados });
    }

    [HttpGet("id/{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        var c = await _db.Clientes.FindAsync(id);
        if (c is null) return NotFound(new { message = "Cliente não encontrado." });
        return Ok(new { message = "Cliente encontrado.", data = c });
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] Cliente c)
    {
        if (string.IsNullOrWhiteSpace(c.Nome))
            return BadRequest(new { message = "Nome é obrigatório." });

        _db.Clientes.Add(c);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Cliente criado.", data = c });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] Cliente c)
    {
        var dbC = await _db.Clientes.FindAsync(id);
        if (dbC is null) return NotFound(new { message = "Cliente não encontrado." });

        dbC.Nome = c.Nome?.Trim() ?? dbC.Nome;
        dbC.Email = c.Email;
        dbC.Telefone = c.Telefone;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Cliente atualizado.", data = dbC });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var dbC = await _db.Clientes.FindAsync(id);
        if (dbC is null) return NotFound(new { message = "Cliente não encontrado." });

        _db.Clientes.Remove(dbC);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Cliente removido." });
    }
}
