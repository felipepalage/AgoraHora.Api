using AgoraHora.Api.Data;
using AgoraHora.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgoraHora.Api.Controllers
{
    [ApiController]
    [Route("api/servicos")]
    public class ServicosController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ServicosController(AppDbContext db) => _db = db;

        [HttpGet("{estabelecimentoId:int}")]
        public async Task<IActionResult> Listar(int estabelecimentoId)
        {
            var lista = await _db.Servicos.AsNoTracking()
                .Where(s => s.EstabelecimentoId == estabelecimentoId && s.Ativo)
                .OrderBy(s => s.Nome)
                .ToListAsync();

            return Ok(new { message = "Serviços listados com sucesso.", data = lista });
        }

        [HttpGet("{estabelecimentoId:int}")]
        public async Task<IActionResult> Listar(int estabelecimentoId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

            var query = _db.Servicos.AsNoTracking()
                .Where(s => s.EstabelecimentoId == estabelecimentoId && s.Ativo)
                .OrderBy(s => s.Nome);

            var total = await query.CountAsync();
            var dados = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new { message = "Serviços listados.", page, pageSize, total, data = dados });
        }

        [HttpGet("id/{id:int}")]
        public async Task<IActionResult> Obter(int id)
        {
            var servico = await _db.Servicos.FindAsync(id);
            if (servico == null)
                return NotFound(new { message = "Serviço não encontrado." });

            return Ok(new { message = "Serviço encontrado.", data = servico });
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] Servico s)
        {
            try
            {
                s.Nome = s.Nome.Trim();

                bool existe = await _db.Servicos.AsNoTracking()
                    .AnyAsync(x => x.EstabelecimentoId == s.EstabelecimentoId && x.Nome == s.Nome);

                if (existe)
                    return Conflict(new { message = "Já existe um serviço com esse nome nesse estabelecimento." });

                _db.Servicos.Add(s);
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    message = "Serviço criado com sucesso.",
                    data = s
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro ao salvar serviço.",
                    detalhe = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro inesperado.",
                    detalhe = ex.Message
                });
            }
        }
    }
}
