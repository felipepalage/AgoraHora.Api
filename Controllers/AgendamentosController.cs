using AgoraHora.Api.Data;
using AgoraHora.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgoraHora.Api.Controllers;

public record AgendamentoCreateDto(int EstabelecimentoId, int ClienteId, int ProfissionalId, int ServicoId, DateTime DtInicio);

[ApiController]
[Route("api/agendamentos")]
public class AgendamentosController : ControllerBase
{
    private readonly AppDbContext _db;
    public AgendamentosController(AppDbContext db) => _db = db;

    // POST /api/agendamentos
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] AgendamentoCreateDto dto)
    {
        try
        {
            var serv = await _db.Servicos.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.ServicoId && s.Ativo);
            if (serv is null) return BadRequest(new { message = "Serviço inválido." });

            var profOk = await _db.Profissionais
                .AnyAsync(p => p.Id == dto.ProfissionalId && p.EstabelecimentoId == dto.EstabelecimentoId && p.Ativo);
            if (!profOk) return BadRequest(new { message = "Profissional inválido." });

            var cliOk = await _db.Clientes
                .AnyAsync(c => c.Id == dto.ClienteId && c.EstabelecimentoId == dto.EstabelecimentoId);
            if (!cliOk) return BadRequest(new { message = "Cliente inválido." });

            var dtFim = dto.DtInicio.AddMinutes(serv.DuracaoMin);

            var conflito = await _db.Agendamentos.AsNoTracking().AnyAsync(a =>
                a.ProfissionalId == dto.ProfissionalId &&
                (a.Status == StatusAgendamento.Pendente || a.Status == StatusAgendamento.Confirmado) &&
                a.DtInicio < dtFim && a.DtFim > dto.DtInicio);

            if (conflito) return Conflict(new { message = "Horário indisponível." });

            var ag = new Agendamento
            {
                EstabelecimentoId = dto.EstabelecimentoId,
                ClienteId = dto.ClienteId,
                ProfissionalId = dto.ProfissionalId,
                ServicoId = dto.ServicoId,
                DtInicio = dto.DtInicio,
                DtFim = dtFim,
                Status = StatusAgendamento.Pendente
            };

            _db.Agendamentos.Add(ag);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Agendamento criado com sucesso.", data = ag });
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new { message = "Erro ao salvar agendamento.", detalhe = ex.InnerException?.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro inesperado.", detalhe = ex.Message });
        }
    }

    // GET /api/agendamentos/profissional?profissionalId=&ini=&fim=&status=
    [HttpGet("profissional")]
    public async Task<IActionResult> ListarPorProfissional(
        [FromQuery] int profissionalId,
        [FromQuery] DateTime ini,
        [FromQuery] DateTime fim,
        [FromQuery] string? status)
    {
        if (profissionalId <= 0 || fim <= ini)
            return BadRequest(new { message = "Parâmetros inválidos." });

        StatusAgendamento? statusEnum = null;
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse(status, true, out StatusAgendamento st))
            statusEnum = st;

        var baseQ =
            from a in _db.Agendamentos.AsNoTracking()
            where a.ProfissionalId == profissionalId
               && a.DtInicio < fim
               && a.DtFim > ini
            select a;

        if (statusEnum.HasValue)
            baseQ = baseQ.Where(a => a.Status == statusEnum.Value);

        var lista = await (
            from a in baseQ
            join c in _db.Clientes.AsNoTracking() on a.ClienteId equals c.Id
            join s in _db.Servicos.AsNoTracking() on a.ServicoId equals s.Id
            join p in _db.Profissionais.AsNoTracking() on a.ProfissionalId equals p.Id
            select new
            {
                a.Id,
                Cliente = c.Nome,
                Servico = s.Nome,
                Profissional = p.Nome,
                a.DtInicio,
                a.DtFim,
                a.Status,
                a.Observacao
            })
            .OrderBy(x => x.DtInicio)
            .ToListAsync();

        var data = lista.Select(x => new
        {
            id = x.Id,
            cliente = x.Cliente,
            servico = x.Servico,
            profissional = x.Profissional,
            dtInicio = x.DtInicio,
            dtFim = x.DtFim,
            status = x.Status switch
            {
                StatusAgendamento.Pendente => "Pendente",
                StatusAgendamento.Confirmado => "Confirmado",
                StatusAgendamento.Cancelado => "Cancelado",
                StatusAgendamento.Concluido => "Concluído"
            },
            observacao = x.Observacao
        });

        return Ok(new { message = "Ok", data });
    }

    // GET /api/agendamentos/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        var ag = await _db.Agendamentos.FindAsync(id);
        if (ag is null) return NotFound(new { message = "Agendamento não encontrado." });
        return Ok(new { message = "Agendamento encontrado.", data = ag });
    }

    // DELETE /api/agendamentos/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancelar(int id)
    {
        var ag = await _db.Agendamentos.FindAsync(id);
        if (ag is null)
            return NotFound(new { message = "Agendamento não encontrado." });

        if (ag.Status == StatusAgendamento.Cancelado)
            return BadRequest(new { message = "Agendamento já está cancelado." });

        if (ag.Status == StatusAgendamento.Concluido)
            return BadRequest(new { message = "Agendamento já foi concluído." });

        ag.Status = StatusAgendamento.Cancelado;
        ag.Observacao = string.IsNullOrEmpty(ag.Observacao)
            ? "Cancelado pelo cliente"
            : $"{ag.Observacao} | Cancelado pelo cliente";

        await _db.SaveChangesAsync();
        return Ok(new { message = "Agendamento cancelado com sucesso.", data = ag });
    }
}
