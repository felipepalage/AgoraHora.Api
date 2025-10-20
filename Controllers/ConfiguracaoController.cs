using AgoraHora.Api.Data;
using AgoraHora.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgoraHora.Api.Controllers
{
    public record ConfigUpdateDto(
        string? Telefone,
        string? Endereco,
        string? Horarios,
        string? Descricao,
        string? NomeEstabelecimento,
        string? ImagemUrl,
        int? AbreMin,
        int? FechaMin,
        bool? Ativo
    );

    [ApiController]
    [Route("api/configuracao")]
    public class ConfiguracaoController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ConfiguracaoController(AppDbContext db) => _db = db;

        [HttpGet("{estabelecimentoId:int}")]
        public async Task<IActionResult> Get(int estabelecimentoId)
        {
            var cfg = await _db.Configuracoes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EstabelecimentoId == estabelecimentoId);

            if (cfg is null)
                return NotFound(new { message = "Configuração não encontrada." });

            // opcional: retornar também dados do Estabelecimento
            var est = await _db.Estabelecimentos
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == estabelecimentoId);

            return Ok(new
            {
                message = "Ok",
                data = new
                {
                    configuracao = cfg,
                    estabelecimento = est is null ? null : new
                    {
                        est.Id,
                        est.Nome,
                        est.Endereco,
                        est.ImagemUrl,
                        est.AvaliacaoMedia,
                        est.AbreMin,
                        est.FechaMin,
                        est.Ativo
                    }
                }
            });
        }

        [HttpPut("{estabelecimentoId:int}")]
        public async Task<IActionResult> Update(int estabelecimentoId, [FromBody] ConfigUpdateDto body)
        {
            // Upsert de Configuração
            var cfg = await _db.Configuracoes
                .FirstOrDefaultAsync(x => x.EstabelecimentoId == estabelecimentoId);

            if (cfg is null)
            {
                cfg = new Configuracao
                {
                    EstabelecimentoId = estabelecimentoId,
                    Telefone = body.Telefone ?? "",
                    Endereco = body.Endereco ?? "",
                    Horarios = body.Horarios ?? "",
                    Descricao = body.Descricao ?? ""
                };
                _db.Configuracoes.Add(cfg);
            }
            else
            {
                cfg.Telefone = body.Telefone ?? cfg.Telefone;
                cfg.Endereco = body.Endereco ?? cfg.Endereco;
                cfg.Horarios = body.Horarios ?? cfg.Horarios;
                cfg.Descricao = body.Descricao ?? cfg.Descricao;
            }

            // Atualiza Estabelecimento quando vier algo opcionalmente
            var est = await _db.Estabelecimentos.FirstOrDefaultAsync(e => e.Id == estabelecimentoId);
            if (est is null)
                return NotFound(new { message = "Estabelecimento não encontrado." });

            if (!string.IsNullOrWhiteSpace(body.NomeEstabelecimento))
                est.Nome = body.NomeEstabelecimento!.Trim();

            // Se endereço vier na config e quiser refletir no Estabelecimento, aplica:
            if (!string.IsNullOrWhiteSpace(body.Endereco))
                est.Endereco = body.Endereco!.Trim();

            if (!string.IsNullOrWhiteSpace(body.ImagemUrl))
                est.ImagemUrl = body.ImagemUrl!.Trim();

            if (body.AbreMin is >= 0 and <= 1440) est.AbreMin = body.AbreMin.Value;
            if (body.FechaMin is >= 0 and <= 1440) est.FechaMin = body.FechaMin.Value;
            if (body.Ativo.HasValue) est.Ativo = body.Ativo.Value;

            if (est.AbreMin == est.FechaMin)
                return BadRequest(new { message = "Horário inválido (AbreMin não pode ser igual a FechaMin)." });

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Atualizado.",
                data = new
                {
                    configuracao = cfg,
                    estabelecimento = new
                    {
                        est.Id,
                        est.Nome,
                        est.Endereco,
                        est.ImagemUrl,
                        est.AvaliacaoMedia,
                        est.AbreMin,
                        est.FechaMin,
                        est.Ativo
                    }
                }
            });
        }
    }
}
