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

    private static bool EstaAberta(int abreMin, int fechaMin, DateTime? agora = null)
    {
        var now = agora ?? DateTime.Now;
        var nowMin = now.Hour * 60 + now.Minute;
        return abreMin < fechaMin
            ? nowMin >= abreMin && nowMin < fechaMin
            : nowMin >= abreMin || nowMin < fechaMin;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? q,
        [FromQuery] bool? abertas,
        [FromQuery] string? order,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var baseQ = _db.Estabelecimentos.AsNoTracking().Where(e => e.Ativo);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            baseQ = baseQ.Where(e => EF.Functions.Like(e.Nome, $"%{term}%"));
        }

        var nowMin = DateTime.Now.Hour * 60 + DateTime.Now.Minute;

        var proj = baseQ.Select(e => new
        {
            id = e.Id,
            nome = e.Nome,
            endereco = e.Endereco,
            avaliacao = e.AvaliacaoMedia,                  // decimal(3,2)
            img = e.ImagemUrl,
            abreMin = e.AbreMin,
            fechaMin = e.FechaMin,
            aberta = e.AbreMin < e.FechaMin
                ? nowMin >= e.AbreMin && nowMin < e.FechaMin
                : nowMin >= e.AbreMin || nowMin < e.FechaMin
        });

        if (abertas == true) proj = proj.Where(x => x.aberta);

        proj = order switch
        {
            "avaliacao" => proj.OrderByDescending(x => x.avaliacao).ThenBy(x => x.nome),
            "abertas" => proj.OrderByDescending(x => x.aberta).ThenBy(x => x.nome),
            _ => proj.OrderBy(x => x.nome)
        };

        var total = await proj.CountAsync();
        var data = await proj.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new { message = "Ok", page, pageSize, total, data });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        var nowMin = DateTime.Now.Hour * 60 + DateTime.Now.Minute;

        var e = await _db.Estabelecimentos.AsNoTracking()
            .Where(x => x.Id == id && x.Ativo)
            .Select(x => new
            {
                id = x.Id,
                nome = x.Nome,
                endereco = x.Endereco,
                img = x.ImagemUrl,
                avaliacao = x.AvaliacaoMedia,
                abreMin = x.AbreMin,
                fechaMin = x.FechaMin,
                aberta = x.AbreMin < x.FechaMin
                    ? nowMin >= x.AbreMin && nowMin < x.FechaMin
                    : nowMin >= x.AbreMin || nowMin < x.FechaMin
            })
            .FirstOrDefaultAsync();

        if (e is null) return NotFound(new { message = "Estabelecimento não encontrado." });
        return Ok(new { message = "Ok", data = e });
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] Estabelecimento e)
    {
        if (string.IsNullOrWhiteSpace(e.Nome) ||
            string.IsNullOrWhiteSpace(e.Endereco) ||
            string.IsNullOrWhiteSpace(e.ImagemUrl))
            return BadRequest(new { message = "Nome, Endereco e ImagemUrl são obrigatórios." });

        if (e.AvaliacaoMedia is < 0 or > 5) return BadRequest(new { message = "AvaliacaoMedia deve estar entre 0 e 5." });
        if (e.AbreMin is < 0 or > 1440 || e.FechaMin is < 0 or > 1440 || e.AbreMin == e.FechaMin)
            return BadRequest(new { message = "Horário inválido (AbreMin/FechaMin)." });

        _db.Estabelecimentos.Add(e);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Estabelecimento criado.", data = e });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] Estabelecimento e)
    {
        var dbE = await _db.Estabelecimentos.FindAsync(id);
        if (dbE is null) return NotFound(new { message = "Estabelecimento não encontrado." });

        dbE.Nome = string.IsNullOrWhiteSpace(e.Nome) ? dbE.Nome : e.Nome.Trim();
        dbE.Endereco = string.IsNullOrWhiteSpace(e.Endereco) ? dbE.Endereco : e.Endereco.Trim();
        dbE.ImagemUrl = string.IsNullOrWhiteSpace(e.ImagemUrl) ? dbE.ImagemUrl : e.ImagemUrl.Trim();

        // Só altera avaliação quando o cliente enviar explicitamente no PUT
        if (e.AvaliacaoMedia is >= 0 and <= 5) dbE.AvaliacaoMedia = e.AvaliacaoMedia;

        if (e.AbreMin is >= 0 and <= 1440) dbE.AbreMin = e.AbreMin;
        if (e.FechaMin is >= 0 and <= 1440) dbE.FechaMin = e.FechaMin;
        dbE.Ativo = e.Ativo;

        if (dbE.AbreMin == dbE.FechaMin)
            return BadRequest(new { message = "Horário inválido (AbreMin não pode ser igual a FechaMin)." });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Estabelecimento atualizado.", data = dbE });
    }

    [HttpPatch("{id:int}/avaliacao")]
    public async Task<IActionResult> AtualizarAvaliacao(int id, [FromBody] AvaliacaoDto dto)
    {
        if (dto?.Nota is null || dto.Nota is < 0 or > 5)
            return BadRequest(new { message = "Nota deve estar entre 0 e 5." });

        var e = await _db.Estabelecimentos.FindAsync(id);
        if (e is null) return NotFound(new { message = "Estabelecimento não encontrado." });

        e.AvaliacaoMedia = (decimal)dto.Nota.Value;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Avaliação atualizada.", data = new { id = e.Id, avaliacao = e.AvaliacaoMedia } });
    }

    public sealed class AvaliacaoDto
    {
        public double? Nota { get; set; }
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
