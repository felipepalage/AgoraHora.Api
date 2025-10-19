using AgoraHora.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AgoraHora.Api.Data
{
    public interface ICurrentTenant
    {
        int? EstabelecimentoId { get; }
    }

    public class AppDbContext : DbContext
    {
        private readonly ICurrentTenant _tenant;

        public AppDbContext(DbContextOptions<AppDbContext> o, ICurrentTenant tenant) : base(o)
        {
            _tenant = tenant;
        }

        public AppDbContext(DbContextOptions<AppDbContext> o) : this(o, new NullTenant()) { }

        private sealed class NullTenant : ICurrentTenant
        {
            public int? EstabelecimentoId => null;
        }

        // ===== DbSets
        public DbSet<Configuracao> Configuracoes => Set<Configuracao>();
        public DbSet<Estabelecimento> Estabelecimentos => Set<Estabelecimento>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Profissional> Profissionais => Set<Profissional>();
        public DbSet<Servico> Servicos => Set<Servico>();
        public DbSet<Agendamento> Agendamentos => Set<Agendamento>();
        public DbSet<Especialidade> Especialidades => Set<Especialidade>();
        public DbSet<ProfissionalEspecialidade> ProfissionalEspecialidades => Set<ProfissionalEspecialidade>();
        public DbSet<Usuario> TB_USUARIO => Set<Usuario>();
        public DbSet<UsuarioEstabelecimento> TB_USUARIO_ESTABELECIMENTO => Set<UsuarioEstabelecimento>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // ===== ESTABELECIMENTO
            b.Entity<Estabelecimento>(e =>
            {
                e.ToTable("TB_ESTABELECIMENTO");
                e.Property(x => x.Nome).IsRequired().HasMaxLength(200);
                e.Property(x => x.Endereco).IsRequired().HasMaxLength(255);
                e.Property(x => x.ImagemUrl).IsRequired().HasMaxLength(500);
                e.Property(x => x.AvaliacaoMedia).HasPrecision(4, 2).HasDefaultValue(0m);
                e.Property(x => x.AbreMin).HasDefaultValue(540);
                e.Property(x => x.FechaMin).HasDefaultValue(1080);
                e.Property(x => x.Ativo).HasDefaultValue(true);
            });

            // ===== CLIENTE
            b.Entity<Cliente>(e =>
            {
                e.ToTable("TB_CLIENTE");
                e.HasIndex(x => new { x.EstabelecimentoId, x.Nome });
                e.Property(x => x.Nome).IsRequired().HasMaxLength(200);
                e.Property(x => x.Email).HasMaxLength(200);
                e.Property(x => x.Telefone).HasMaxLength(30);

                e.HasOne<Estabelecimento>()
                 .WithMany()
                 .HasForeignKey(x => x.EstabelecimentoId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== PROFISSIONAL
            b.Entity<Profissional>(e =>
            {
                e.ToTable("TB_PROFISSIONAL");
                e.HasIndex(x => new { x.EstabelecimentoId, x.Nome });
                e.Property(x => x.Nome).IsRequired().HasMaxLength(200);
                e.Property(x => x.Especialidade).HasMaxLength(200);
                e.Property(x => x.Ativo).HasDefaultValue(true);

                e.HasOne<Estabelecimento>()
                 .WithMany()
                 .HasForeignKey(x => x.EstabelecimentoId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== ESPECIALIDADE
            b.Entity<Especialidade>(e =>
            {
                e.ToTable("TB_ESPECIALIDADE");
                e.Property(x => x.Nome).IsRequired().HasMaxLength(100);
                e.HasIndex(x => x.Nome).IsUnique();
            });

            // ===== PROFISSIONAL x ESPECIALIDADE (N:N)
            b.Entity<ProfissionalEspecialidade>(e =>
            {
                e.ToTable("TB_PROFISSIONAL_ESPECIALIDADE");
                e.HasKey(x => new { x.ProfissionalId, x.EspecialidadeId });

                e.HasOne(x => x.Profissional)
                 .WithMany(p => p.ProfissionalEspecialidades)
                 .HasForeignKey(x => x.ProfissionalId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Especialidade)
                 .WithMany(es => es.ProfissionalEspecialidades)
                 .HasForeignKey(x => x.EspecialidadeId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.ProfissionalId);
                e.HasIndex(x => x.EspecialidadeId);
            });

            // ===== SERVICO
            b.Entity<Servico>(e =>
            {
                e.ToTable("TB_SERVICO");
                e.HasIndex(x => new { x.EstabelecimentoId, x.Nome }).IsUnique();
                e.Property(x => x.Nome).IsRequired().HasMaxLength(200);
                e.Property(x => x.DuracaoMin).HasDefaultValue(30);
                e.Property(x => x.Preco).HasColumnType("decimal(10,2)");
                e.Property(x => x.Ativo).HasDefaultValue(true);

                e.HasOne<Estabelecimento>()
                 .WithMany()
                 .HasForeignKey(x => x.EstabelecimentoId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== AGENDAMENTO
            b.Entity<Agendamento>(e =>
            {
                e.ToTable("TB_AGENDAMENTO");
                e.Property(x => x.Status).HasConversion<int>();
                e.HasIndex(x => new { x.ProfissionalId, x.DtInicio, x.DtFim });

                e.HasOne<Estabelecimento>()
                 .WithMany()
                 .HasForeignKey(x => x.EstabelecimentoId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne<Cliente>()
                 .WithMany()
                 .HasForeignKey(x => x.ClienteId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<Profissional>()
                 .WithMany()
                 .HasForeignKey(x => x.ProfissionalId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<Servico>()
                 .WithMany()
                 .HasForeignKey(x => x.ServicoId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== CONFIGURAÇÃO
            b.Entity<Configuracao>(e =>
            {
                e.ToTable("TB_CONFIGURACAO");
                e.Property(x => x.Telefone).HasMaxLength(30);
                e.Property(x => x.Endereco).HasMaxLength(255);
                e.Property(x => x.Horarios).HasMaxLength(500);
                e.Property(x => x.Descricao).HasMaxLength(1000);

                e.HasOne<Estabelecimento>()
                 .WithOne()
                 .HasForeignKey<Configuracao>(x => x.EstabelecimentoId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== USUÁRIO
            b.Entity<Usuario>(e =>
            {
                e.ToTable("TB_USUARIO");
                e.Property(x => x.Email).HasMaxLength(200).IsRequired();
                e.Property(x => x.Nome).HasMaxLength(200).IsRequired();
                e.Property(x => x.SenhaHash).HasMaxLength(200).IsRequired();
                e.Property(x => x.Ativo).HasDefaultValue(true);
                e.HasIndex(x => x.Email).IsUnique();
            });

            // ===== USUÁRIO x ESTABELECIMENTO
            b.Entity<UsuarioEstabelecimento>(e =>
            {
                e.ToTable("TB_USUARIO_ESTABELECIMENTO");
                e.HasKey(x => new { x.UsuarioId, x.EstabelecimentoId });
                e.Property(x => x.Papel).HasMaxLength(50).HasDefaultValue("owner");

                e.HasOne(x => x.Usuario)
                 .WithMany(u => u.Estabelecimentos)
                 .HasForeignKey(x => x.UsuarioId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Estabelecimento)
                 .WithMany()
                 .HasForeignKey(x => x.EstabelecimentoId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== TENANT FILTER
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
