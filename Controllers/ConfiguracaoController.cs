using AgoraHora.Api.Data;
using AgoraHora.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgoraHora.Api.Controllers
{
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

            if (cfg == null)
                return NotFound(new { message = "Configuração não encontrada." });

            return Ok(new { message = "Configuração encontrada.", data = cfg });
        }

        [HttpPut("{estabelecimentoId:int}")]
        public async Task<IActionResult> Update(int estabelecimentoId, [FromBody] Configuracao model)
        {
            var cfg = await _db.Configuracoes
                .FirstOrDefaultAsync(x => x.EstabelecimentoId == estabelecimentoId);

            if (cfg is null)
            {
                model.EstabelecimentoId = estabelecimentoId;
                _db.Configuracoes.Add(model);
                await _db.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { estabelecimentoId }, new { message = "Criado.", data = model });
            }

            cfg.Telefone = model.Telefone;
            cfg.Endereco = model.Endereco;
            cfg.Horarios = model.Horarios;
            cfg.Descricao = model.Descricao;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Atualizado.", data = cfg });
        }

    }
}
