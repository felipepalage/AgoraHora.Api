using AgoraHora.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AgoraHora.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> o) : base(o) { }

    public DbSet<Estabelecimento> Estabelecimentos => Set<Estabelecimento>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Profissional> Profissionais => Set<Profissional>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Estabelecimento>().ToTable("TB_ESTABELECIMENTO");

        b.Entity<Cliente>().ToTable("TB_CLIENTE");
        b.Entity<Cliente>().HasIndex(x => new { x.EstabelecimentoId, x.Nome });

        b.Entity<Profissional>().ToTable("TB_PROFISSIONAL");
        b.Entity<Profissional>().HasIndex(x => new { x.EstabelecimentoId, x.Nome });

        b.Entity<Servico>().ToTable("TB_SERVICO");
        b.Entity<Servico>().HasIndex(x => new { x.EstabelecimentoId, x.Nome }).IsUnique();

        b.Entity<Agendamento>().ToTable("TB_AGENDAMENTO");
        b.Entity<Agendamento>().Property(x => x.Status).HasConversion<int>();
        b.Entity<Agendamento>().HasIndex(x => new { x.ProfissionalId, x.DtInicio, x.DtFim });
    }
}
