using AgoraHora.Api.Data;
using AgoraHora.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgoraHora.Api.Controllers;

[ApiController]
[Route("api/servicos")]
public class ServicosController : ControllerBase
{
    private readonly AppDbContext _db;
    public ServicosController(AppDbContext db) => _db = db;

    [HttpGet("{estabelecimentoId:int}")]
    public async Task<IActionResult> Listar(int estabelecimentoId)
    {
        var dados = await _db.Servicos.AsNoTracking()
            .Where(s => s.EstabelecimentoId == estabelecimentoId && s.Ativo)
            .OrderBy(s => s.Nome)
            .Select(s => new { id = s.Id, nome = s.Nome, preco = s.Preco, duracao = s.DuracaoMin })
            .ToListAsync();
        return Ok(new { data = dados });
    }

    [HttpGet("id/{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        var s = await _db.Servicos.FindAsync(id);
        if (s is null) return NotFound(new { message = "Serviço não encontrado." });
        return Ok(new { message = "Serviço encontrado.", data = s });
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] Servico s)
    {
        if (string.IsNullOrWhiteSpace(s.Nome))
            return BadRequest(new { message = "Nome é obrigatório." });

        s.Nome = s.Nome.Trim();

        var existe = await _db.Servicos.AsNoTracking()
            .AnyAsync(x => x.EstabelecimentoId == s.EstabelecimentoId && x.Nome == s.Nome);

        if (existe) return Conflict(new { message = "Já existe um serviço com esse nome nesse estabelecimento." });

        _db.Servicos.Add(s);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Serviço criado com sucesso.", data = s });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] Servico s)
    {
        var dbS = await _db.Servicos.FindAsync(id);
        if (dbS is null) return NotFound(new { message = "Serviço não encontrado." });

        var novoNome = s.Nome?.Trim() ?? dbS.Nome;
        if (!string.Equals(novoNome, dbS.Nome, StringComparison.OrdinalIgnoreCase))
        {
            var conflito = await _db.Servicos.AsNoTracking().AnyAsync(x =>
                x.EstabelecimentoId == dbS.EstabelecimentoId &&
                x.Nome == novoNome &&
                x.Id != dbS.Id);
            if (conflito) return Conflict(new { message = "Já existe um serviço com esse nome nesse estabelecimento." });
            dbS.Nome = novoNome;
        }

        if (s.DuracaoMin > 0) dbS.DuracaoMin = s.DuracaoMin;
        if (s.Preco >= 0) dbS.Preco = s.Preco;
        dbS.Ativo = s.Ativo || dbS.Ativo;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Serviço atualizado.", data = new { dbS.Id, dbS.Nome, dbS.Preco, dbS.DuracaoMin } });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var dbS = await _db.Servicos.FindAsync(id);
        if (dbS is null) return NotFound(new { message = "Serviço não encontrado." });

        _db.Servicos.Remove(dbS);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Serviço removido." });
    }
}
