using AgoraHora.Api.Data;
using AgoraHora.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgoraHora.Api.Controllers;

[ApiController]
[Route("api/relatorios")]
public class RelatoriosController : ControllerBase
{
    private readonly AppDbContext _db;
    public RelatoriosController(AppDbContext db) => _db = db;

    [HttpGet("resumo")]
    public async Task<IActionResult> Resumo([FromQuery] int estabelecimentoId, [FromQuery] DateTime ini, [FromQuery] DateTime fim)
    {
        var q = from a in _db.Agendamentos.AsNoTracking()
                join s in _db.Servicos.AsNoTracking() on a.ServicoId equals s.Id
                where a.EstabelecimentoId == estabelecimentoId && a.DtInicio >= ini && a.DtInicio < fim
                select new { a.Status, s.Preco, s.Nome, s.DuracaoMin };

        var lista = await q.ToListAsync();

        int agendados = lista.Count;
        int pendentes = lista.Count(x => x.Status == StatusAgendamento.Pendente);
        int confirmados = lista.Count(x => x.Status == StatusAgendamento.Confirmado);
        int cancelados = lista.Count(x => x.Status == StatusAgendamento.Cancelado);
        int concluidos = lista.Count(x => x.Status == StatusAgendamento.Concluido);

        decimal faturamentoEstimado = lista
            .Where(x => x.Status == StatusAgendamento.Confirmado || x.Status == StatusAgendamento.Concluido)
            .Sum(x => x.Preco);

        int minutosTotais = lista
            .Where(x => x.Status != StatusAgendamento.Cancelado)
            .Sum(x => x.DuracaoMin);

        var topServicos = lista
            .GroupBy(x => x.Nome)
            .Select(g => new { servico = g.Key, qtd = g.Count() })
            .OrderByDescending(x => x.qtd)
            .Take(5)
            .ToList();

        return Ok(new
        {
            message = "Resumo do período.",
            data = new
            {
                periodo = new { ini, fim },
                agendados,
                pendentes,
                confirmados,
                cancelados,
                concluidos,
                faturamentoEstimado,
                minutosTotais,
                topServicos
            }
        });
    }

    [HttpGet("profissional")]
    public async Task<IActionResult> PorProfissional([FromQuery] int profissionalId, [FromQuery] DateTime ini, [FromQuery] DateTime fim)
    {
        var q = from a in _db.Agendamentos.AsNoTracking()
                join s in _db.Servicos.AsNoTracking() on a.ServicoId equals s.Id
                join c in _db.Clientes.AsNoTracking() on a.ClienteId equals c.Id
                where a.ProfissionalId == profissionalId && a.DtInicio >= ini && a.DtInicio < fim
                orderby a.DtInicio
                select new
                {
                    a.Id,
                    a.DtInicio,
                    a.DtFim,
                    a.Status,
                    servicoId = s.Id,
                    servico = s.Nome,
                    preco = s.Preco,
                    clienteId = c.Id,
                    cliente = c.Nome
                };

        var itens = await q.ToListAsync();

        var resumo = new
        {
            total = itens.Count,
            pendentes = itens.Count(x => x.Status == StatusAgendamento.Pendente),
            confirmados = itens.Count(x => x.Status == StatusAgendamento.Confirmado),
            cancelados = itens.Count(x => x.Status == StatusAgendamento.Cancelado),
            concluidos = itens.Count(x => x.Status == StatusAgendamento.Concluido),
            faturamento = itens
                .Where(x => x.Status == StatusAgendamento.Confirmado || x.Status == StatusAgendamento.Concluido)
                .Sum(x => x.preco)
        };

        return Ok(new { message = "Agenda do profissional.", data = new { resumo, itens } });
    }

    [HttpGet("cliente")]
    public async Task<IActionResult> PorCliente([FromQuery] int clienteId, [FromQuery] DateTime ini, [FromQuery] DateTime fim)
    {
        var q = from a in _db.Agendamentos.AsNoTracking()
                join s in _db.Servicos.AsNoTracking() on a.ServicoId equals s.Id
                where a.ClienteId == clienteId && a.DtInicio >= ini && a.DtInicio < fim
                orderby a.DtInicio descending
                select new
                {
                    a.Id,
                    a.DtInicio,
                    a.DtFim,
                    a.Status,
                    servicoId = s.Id,
                    servico = s.Nome,
                    preco = s.Preco
                };

        var historico = await q.ToListAsync();

        return Ok(new { message = "Histórico do cliente.", data = historico });
    }
}
