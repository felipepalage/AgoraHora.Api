using AgoraHora.Api.Data;
using AgoraHora.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace AgoraHora.Api.Controllers;

[ApiController]
[Route("api/estabelecimentos")]
public class EstabelecimentosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public EstabelecimentosController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

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
            avaliacao = e.AvaliacaoMedia,
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

        if (ModelState.ContainsKey(nameof(Estabelecimento.Nome)) &&
            !string.IsNullOrWhiteSpace(e.Nome))
            dbE.Nome = e.Nome.Trim();

        if (ModelState.ContainsKey(nameof(Estabelecimento.Endereco)) &&
            !string.IsNullOrWhiteSpace(e.Endereco))
            dbE.Endereco = e.Endereco.Trim();

        if (ModelState.ContainsKey(nameof(Estabelecimento.ImagemUrl)) &&
            !string.IsNullOrWhiteSpace(e.ImagemUrl))
            dbE.ImagemUrl = e.ImagemUrl.Trim();

        if (ModelState.ContainsKey(nameof(Estabelecimento.AvaliacaoMedia)) &&
            e.AvaliacaoMedia is >= 0 and <= 5)
            dbE.AvaliacaoMedia = e.AvaliacaoMedia;

        if (ModelState.ContainsKey(nameof(Estabelecimento.AbreMin)) &&
            e.AbreMin is >= 0 and <= 1440)
            dbE.AbreMin = e.AbreMin;

        if (ModelState.ContainsKey(nameof(Estabelecimento.FechaMin)) &&
            e.FechaMin is >= 0 and <= 1440)
            dbE.FechaMin = e.FechaMin;

        if (ModelState.ContainsKey(nameof(Estabelecimento.Ativo)))
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

    public sealed class AvaliacaoDto { public double? Nota { get; set; } }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var dbE = await _db.Estabelecimentos.FindAsync(id);
        if (dbE is null) return NotFound(new { message = "Estabelecimento não encontrado." });

        _db.Estabelecimentos.Remove(dbE);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Estabelecimento removido." });
    }

    // ===== Upload de logo (salva arquivo local e grava só a URL em ImagemUrl)
    [HttpPost("{id:int}/logo")]
    [RequestSizeLimit(5_000_000)] // 5 MB
    public async Task<IActionResult> UploadLogo(int id, IFormFile? file)
    {
        var e = await _db.Estabelecimentos.FindAsync(id);
        if (e is null) return NotFound(new { message = "Estabelecimento não encontrado." });
        if (file is null || file.Length == 0) return BadRequest(new { message = "Arquivo obrigatório." });
        if (file.Length > 5_000_000) return BadRequest(new { message = "Máx. 5MB." });

        // valida tipo
        var okTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "image/png","image/jpeg","image/webp","image/svg+xml" };

        var provider = new FileExtensionContentTypeProvider();
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!provider.TryGetContentType("x" + ext, out var guessed)) guessed = file.ContentType;
        var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? guessed : file.ContentType;
        if (!okTypes.Contains(contentType)) return BadRequest(new { message = "Tipo inválido. Use PNG/JPG/WEBP/SVG." });

        // diretório físico: wwwroot/uploads/estabelecimentos/{id}/
        var baseDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "estabelecimentos", id.ToString());
        Directory.CreateDirectory(baseDir);

        // nome fixo + bust na querystring
        var fileName = "logo" + ext;
        var fullPath = Path.Combine(baseDir, fileName);
        await using (var fs = System.IO.File.Create(fullPath))
            await file.CopyToAsync(fs);

        var url = $"/uploads/estabelecimentos/{id}/{fileName}?v={DateTime.UtcNow.Ticks}";
        e.ImagemUrl = url;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Logo atualizada.", url });
    }
}
