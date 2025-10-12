using AgoraHora.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AgoraHora.Api.Data
{
    // Contexto do tenant atual (preenchido no runtime via HttpContext)
    public interface ICurrentTenant
    {
        int? EstabelecimentoId { get; }
    }

    public class AppDbContext : DbContext
    {
        private readonly ICurrentTenant _tenant;

        // Runtime (injeta ICurrentTenant via DI)
        public AppDbContext(DbContextOptions<AppDbContext> o, ICurrentTenant tenant) : base(o)
        {
            _tenant = tenant;
        }

        // Design-time (migrations / factory)
        public AppDbContext(DbContextOptions<AppDbContext> o) : this(o, new NullTenant()) { }

        private sealed class NullTenant : ICurrentTenant
        {
            public int? EstabelecimentoId => null;
        }

        // ===== DbSets existentes
        public DbSet<Configuracao> Configuracoes => Set<Configuracao>();
        public DbSet<Estabelecimento> Estabelecimentos => Set<Estabelecimento>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Profissional> Profissionais => Set<Profissional>();
        public DbSet<Servico> Servicos => Set<Servico>();
        public DbSet<Agendamento> Agendamentos => Set<Agendamento>();

        // ===== Autenticação / Multi-tenant
        public DbSet<Usuario> TB_USUARIO => Set<Usuario>();
        public DbSet<UsuarioEstabelecimento> TB_USUARIO_ESTABELECIMENTO => Set<UsuarioEstabelecimento>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // ===== ESTABELECIMENTO
            b.Entity<Estabelecimento>().ToTable("TB_ESTABELECIMENTO");

            // ===== CLIENTE
            b.Entity<Cliente>().ToTable("TB_CLIENTE");
            b.Entity<Cliente>().HasIndex(x => new { x.EstabelecimentoId, x.Nome });
            b.Entity<Cliente>()
                .HasOne<Estabelecimento>()
                .WithMany()
                .HasForeignKey(x => x.EstabelecimentoId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== PROFISSIONAL
            b.Entity<Profissional>().ToTable("TB_PROFISSIONAL");
            b.Entity<Profissional>().HasIndex(x => new { x.EstabelecimentoId, x.Nome });
            b.Entity<Profissional>()
                .HasOne<Estabelecimento>()
                .WithMany()
                .HasForeignKey(x => x.EstabelecimentoId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== SERVICO
            b.Entity<Servico>().ToTable("TB_SERVICO");
            b.Entity<Servico>().HasIndex(x => new { x.EstabelecimentoId, x.Nome }).IsUnique();
            b.Entity<Servico>()
                .HasOne<Estabelecimento>()
                .WithMany()
                .HasForeignKey(x => x.EstabelecimentoId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== AGENDAMENTO
            b.Entity<Agendamento>().ToTable("TB_AGENDAMENTO");
            b.Entity<Agendamento>().Property(x => x.Status).HasConversion<int>();
            b.Entity<Agendamento>().HasIndex(x => new { x.ProfissionalId, x.DtInicio, x.DtFim });

            b.Entity<Agendamento>()
                .HasOne<Estabelecimento>()
                .WithMany()
                .HasForeignKey(x => x.EstabelecimentoId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Agendamento>()
                .HasOne<Cliente>()
                .WithMany()
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Agendamento>()
                .HasOne<Profissional>()
                .WithMany()
                .HasForeignKey(x => x.ProfissionalId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Agendamento>()
                .HasOne<Servico>()
                .WithMany()
                .HasForeignKey(x => x.ServicoId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== CONFIGURAÇÃO
            b.Entity<Configuracao>().ToTable("TB_CONFIGURACAO");
            b.Entity<Configuracao>()
                .HasOne<Estabelecimento>()
                .WithOne()
                .HasForeignKey<Configuracao>(x => x.EstabelecimentoId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== USUÁRIO
            b.Entity<Usuario>().ToTable("TB_USUARIO");
            b.Entity<Usuario>().Property(x => x.Email).HasMaxLength(200).IsRequired();
            b.Entity<Usuario>().Property(x => x.Nome).HasMaxLength(200).IsRequired();
            b.Entity<Usuario>().Property(x => x.SenhaHash).HasMaxLength(200).IsRequired();
            b.Entity<Usuario>().Property(x => x.Ativo).HasDefaultValue(true);
            b.Entity<Usuario>().HasIndex(x => x.Email).IsUnique();

            // ===== USUÁRIO x ESTABELECIMENTO
            b.Entity<UsuarioEstabelecimento>().ToTable("TB_USUARIO_ESTABELECIMENTO");
            b.Entity<UsuarioEstabelecimento>().HasKey(x => new { x.UsuarioId, x.EstabelecimentoId });
            b.Entity<UsuarioEstabelecimento>().Property(x => x.Papel).HasMaxLength(50).HasDefaultValue("owner");

            b.Entity<UsuarioEstabelecimento>()
                .HasOne(x => x.Usuario)
                .WithMany(u => u.Estabelecimentos)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<UsuarioEstabelecimento>()
                .HasOne(x => x.Estabelecimento)
                .WithMany()
                .HasForeignKey(x => x.EstabelecimentoId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== Filtro global por tenant (aplicado somente em runtime)
            if (_tenant?.EstabelecimentoId is int tenantId)
            {
                b.Entity<Cliente>().HasQueryFilter(e => e.EstabelecimentoId == tenantId);
                b.Entity<Profissional>().HasQueryFilter(e => e.EstabelecimentoId == tenantId);
                b.Entity<Servico>().HasQueryFilter(e => e.EstabelecimentoId == tenantId);
                b.Entity<Agendamento>().HasQueryFilter(e => e.EstabelecimentoId == tenantId);
                b.Entity<Configuracao>().HasQueryFilter(e => e.EstabelecimentoId == tenantId);
            }
        }
    }
}
