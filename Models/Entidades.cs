using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AgoraHora.Api.Models;

public class Estabelecimento
{
    public int Id { get; set; }

    [Required] public string Nome { get; set; } = null!;

    [Required] public string Endereco { get; set; } = string.Empty;
    [Required] public string ImagemUrl { get; set; } = string.Empty;

    [Precision(4, 2)]
    public decimal AvaliacaoMedia { get; set; } = 0m; 

    public int AbreMin { get; set; } = 540;
    public int FechaMin { get; set; } = 1080;

    public bool Ativo { get; set; } = true;
}
public class Cliente
{
    public int Id { get; set; }
    public int EstabelecimentoId { get; set; }
    [Required] public string Nome { get; set; } = null!;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
}

public class Profissional
{
    public int Id { get; set; }
    public int EstabelecimentoId { get; set; }
    [Required] public string Nome { get; set; } = null!;
    public bool Ativo { get; set; } = true;
}

public class Servico
{
    public int Id { get; set; }
    public int EstabelecimentoId { get; set; }
    [Required] public string Nome { get; set; } = null!;
    public int DuracaoMin { get; set; } = 30;
    public decimal Preco { get; set; }
    public bool Ativo { get; set; } = true;
}
public class Configuracao
{
    public int Id { get; set; }
    public int EstabelecimentoId { get; set; }
    public string? Telefone { get; set; }
    public string? Endereco { get; set; }
    public string? Horarios { get; set; }
    public string? Descricao { get; set; }
}

public class Agendamento
{
    public int Id { get; set; }
    public int EstabelecimentoId { get; set; }
    public int ClienteId { get; set; }
    public int ProfissionalId { get; set; }
    public int ServicoId { get; set; }
    public DateTime DtInicio { get; set; }
    public DateTime DtFim { get; set; }
    public StatusAgendamento Status { get; set; } = StatusAgendamento.Pendente;
    public string? Observacao { get; set; }
}
