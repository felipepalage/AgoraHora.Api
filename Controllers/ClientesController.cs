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

    // GET /api/clientes/{estabelecimentoId}?page=1&pageSize=20
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

    // GET /api/clientes/id/123
    [HttpGet("id/{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        var c = await _db.Clientes.FindAsync(id);
        if (c is null) return NotFound(new { message = "Cliente não encontrado." });
        return Ok(new { message = "Cliente encontrado.", data = c });
    }

    // POST /api/clientes
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] Cliente c)
    {
        if (string.IsNullOrWhiteSpace(c.Nome) || c.EstabelecimentoId <= 0)
            return BadRequest(new { message = "EstabelecimentoId e Nome são obrigatórios." });

        c.Nome = c.Nome.Trim();
        if (!string.IsNullOrWhiteSpace(c.Email)) c.Email = c.Email.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(c.Telefone)) c.Telefone = new string(c.Telefone.Where(char.IsDigit).ToArray());

        _db.Clientes.Add(c);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Cliente criado.", data = c });
    }

    // PUT /api/clientes/123
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] Cliente c)
    {
        var dbC = await _db.Clientes.FindAsync(id);
        if (dbC is null) return NotFound(new { message = "Cliente não encontrado." });

        dbC.Nome = string.IsNullOrWhiteSpace(c.Nome) ? dbC.Nome : c.Nome.Trim();
        dbC.Email = string.IsNullOrWhiteSpace(c.Email) ? dbC.Email : c.Email.Trim().ToLowerInvariant();
        dbC.Telefone = string.IsNullOrWhiteSpace(c.Telefone) ? dbC.Telefone : new string(c.Telefone.Where(char.IsDigit).ToArray());

        await _db.SaveChangesAsync();
        return Ok(new { message = "Cliente atualizado.", data = dbC });
    }

    // DELETE /api/clientes/123
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var dbC = await _db.Clientes.FindAsync(id);
        if (dbC is null) return NotFound(new { message = "Cliente não encontrado." });

        _db.Clientes.Remove(dbC);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Cliente removido." });
    }

    // POST /api/clientes/ensure
    // Busca por (email) OU (telefone). Se não encontrar, cria.
    [HttpPost("ensure")]
    public async Task<IActionResult> Ensure([FromBody] Cliente dto)
    {
        if (dto.EstabelecimentoId <= 0 || string.IsNullOrWhiteSpace(dto.Nome))
            return BadRequest(new { message = "EstabelecimentoId e Nome são obrigatórios." });

        var estId = dto.EstabelecimentoId;
        var email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();
        var fone = string.IsNullOrWhiteSpace(dto.Telefone) ? null : new string(dto.Telefone.Where(char.IsDigit).ToArray());

        if (email is null && fone is null)
            return BadRequest(new { message = "Informe Email ou Telefone para identificar o cliente." });

        IQueryable<Cliente> q = _db.Clientes.AsQueryable().Where(c => c.EstabelecimentoId == estId);

        if (email is not null) q = q.Where(c => c.Email == email);
        else q = q.Where(c => c.Telefone == fone);

        var c = await q.FirstOrDefaultAsync();
        if (c is null)
        {
            c = new Cliente
            {
                EstabelecimentoId = estId,
                Nome = dto.Nome.Trim(),
                Email = email,
                Telefone = fone
            };
            _db.Clientes.Add(c);
            await _db.SaveChangesAsync();
        }

        return Ok(new { data = c });
    }
}
