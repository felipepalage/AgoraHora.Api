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

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] AgendamentoCreateDto dto)
    {
        try
        {
            var serv = await _db.Servicos.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.ServicoId && s.Ativo);
            if (serv is null) return BadRequest(new { message = "Serviço inválido." });

            // valida existência básica
            if (!await _db.Profissionais.AnyAsync(p => p.Id == dto.ProfissionalId && p.EstabelecimentoId == dto.EstabelecimentoId && p.Ativo))
                return BadRequest(new { message = "Profissional inválido." });
            if (!await _db.Clientes.AnyAsync(c => c.Id == dto.ClienteId && c.EstabelecimentoId == dto.EstabelecimentoId))
                return BadRequest(new { message = "Cliente inválido." });

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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        var ag = await _db.Agendamentos.FindAsync(id);
        if (ag is null) return NotFound(new { message = "Agendamento não encontrado." });
        return Ok(new { message = "Agendamento encontrado.", data = ag });
    }
}
