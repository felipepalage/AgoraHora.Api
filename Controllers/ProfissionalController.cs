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

    // ===== DTOs
    public sealed record ProfissionalListDto(
        int Id,
        int EstabelecimentoId,
        string Nome,
        bool Ativo,
        string? Especialidade,          // legado
        string[] Especialidades         // N:N
    );

    public sealed record AtualizarEspecialidadesRequest(string[] Nomes);

    // ===== LISTAR (com especialidades N:N)
    [HttpGet("{estabelecimentoId:int}")]
    public async Task<IActionResult> Listar(int estabelecimentoId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var baseQ = _db.Profissionais
            .AsNoTracking()
            .Where(p => p.EstabelecimentoId == estabelecimentoId)
            .OrderBy(p => p.Nome)
            .Select(p => new ProfissionalListDto(
                p.Id,
                p.EstabelecimentoId,
                p.Nome,
                p.Ativo,
                p.Especialidade,
                p.ProfissionalEspecialidades
                    .Select(pe => pe.Especialidade.Nome)
                    .ToArray()
            ));

        var total = await baseQ.CountAsync();
        var dados = await baseQ.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new { message = "Profissionais listados.", page, pageSize, total, data = dados });
    }

    // ===== OBTER (com especialidades N:N)
    [HttpGet("id/{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        var p = await _db.Profissionais
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(p => new ProfissionalListDto(
                p.Id,
                p.EstabelecimentoId,
                p.Nome,
                p.Ativo,
                p.Especialidade,
                p.ProfissionalEspecialidades
                    .Select(pe => pe.Especialidade.Nome)
                    .ToArray()
            ))
            .FirstOrDefaultAsync();

        if (p is null) return NotFound(new { message = "Profissional não encontrado." });
        return Ok(new { message = "Profissional encontrado.", data = p });
    }

    // ===== CRIAR
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] Profissional p)
    {
        if (string.IsNullOrWhiteSpace(p.Nome))
            return BadRequest(new { message = "Nome é obrigatório." });

        _db.Profissionais.Add(p);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Profissional criado.", data = p });
    }

    // ===== ATUALIZAR DADOS BÁSICOS (legado mantém campo texto)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] Profissional p)
    {
        var dbP = await _db.Profissionais.FindAsync(id);
        if (dbP is null) return NotFound(new { message = "Profissional não encontrado." });

        dbP.Nome = p.Nome?.Trim() ?? dbP.Nome;
        dbP.Especialidade = p.Especialidade?.Trim() ?? dbP.Especialidade; // legado
        dbP.Ativo = p.Ativo;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Profissional atualizado.", data = dbP });
    }

    // ===== REMOVER
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var dbP = await _db.Profissionais.FindAsync(id);
        if (dbP is null) return NotFound(new { message = "Profissional não encontrado." });

        _db.Profissionais.Remove(dbP);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Profissional removido." });
    }

    // ===== ESPECIALIDADES - LER
    [HttpGet("id/{id:int}/especialidades")]
    public async Task<IActionResult> ObterEspecialidades(int id)
    {
        var nomes = await _db.ProfissionalEspecialidades
            .Where(x => x.ProfissionalId == id)
            .Select(x => x.Especialidade.Nome)
            .OrderBy(x => x)
            .ToListAsync();

        return Ok(new { message = "OK", data = nomes });
    }

    // ===== ESPECIALIDADES - ATUALIZAR LISTA COMPLETA (cria se não existir)
    [HttpPut("id/{id:int}/especialidades")]
    public async Task<IActionResult> AtualizarEspecialidades(int id, [FromBody] AtualizarEspecialidadesRequest req)
    {
        var prof = await _db.Profissionais.FindAsync(id);
        if (prof is null) return NotFound(new { message = "Profissional não encontrado." });

        var nomes = (req?.Nomes ?? Array.Empty<string>())
            .Select(n => (n ?? "").Trim())
            .Where(n => n.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        // Resolve existentes
        var existentes = await _db.Especialidades
            .Where(e => nomes.Contains(e.Nome))
            .ToListAsync();

        // Cria faltantes
        var faltantes = nomes
            .Except(existentes.Select(e => e.Nome), StringComparer.OrdinalIgnoreCase)
            .Select(n => new Especialidade { Nome = n })
            .ToList();

        if (faltantes.Count > 0)
        {
            _db.Especialidades.AddRange(faltantes);
            await _db.SaveChangesAsync();
            existentes.AddRange(faltantes);
        }

        var novosIds = existentes.Select(e => e.Id).ToHashSet();

        // Regrava vínculos
        var antigos = _db.ProfissionalEspecialidades.Where(pe => pe.ProfissionalId == id);
        _db.ProfissionalEspecialidades.RemoveRange(antigos);
        _db.ProfissionalEspecialidades.AddRange(novosIds.Select(esId => new ProfissionalEspecialidade
        {
            ProfissionalId = id,
            EspecialidadeId = esId
        }));

        await _db.SaveChangesAsync();
        return Ok(new { message = "Especialidades atualizadas.", data = nomes });
    }
}
