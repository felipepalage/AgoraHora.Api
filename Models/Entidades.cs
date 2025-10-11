using System.ComponentModel.DataAnnotations;

namespace AgoraHora.Api.Models;

public class Estabelecimento { public int Id { get; set; } [Required] public string Nome { get; set; } = null!; }

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
