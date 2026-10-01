using Microsoft.EntityFrameworkCore;
using MatutosDomain;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;

namespace MatutosApi.Infraestrutura
{
    public class MatutosDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Barbeiro> Barbeiros { get; set; }
        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Telefone> Telefones { get; set; }
        public DbSet<UsuarioTelefone> UsuarioTelefones { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<Agendamento> Agendamentos { get; set; }
        public DbSet<Agendamento_Servico> Agendamento_Servicos { get; set; }
        public DbSet<Servico_Imagem> Servico_Imagens { get; set; }
        public DbSet<Blacklist> Blacklists { get; set; }
        public DbSet<Usuario_Blacklist> Usuario_Blacklists { get; set; }
        public DbSet<Configura_Notificacao> Configura_Notificacoes { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }
        public DbSet<Tipo_Evento> Tipo_Eventos { get; set; }
        public DbSet<AuditoriaLog> AuditoriaLogs { get; set; }

        // CORREÇÃO: Adicionado : base(options)
        public MatutosDbContext(DbContextOptions<MatutosDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tabelas do banco de dados
            modelBuilder.Entity<Cliente>().ToTable("cliente");
            modelBuilder.Entity<Barbeiro>().ToTable("barbeiro");
            modelBuilder.Entity<Administrador>().ToTable("administrador");
            modelBuilder.Entity<Telefone>().ToTable("telefone");
            modelBuilder.Entity<UsuarioTelefone>().ToTable("usuario_telefone");
            modelBuilder.Entity<Agendamento>().ToTable("agendamento");
            modelBuilder.Entity<Agendamento_Servico>().ToTable("agendamento_servico");
            modelBuilder.Entity<Servico>().ToTable("servico");
            modelBuilder.Entity<Blacklist>().ToTable("blacklist");
            modelBuilder.Entity<Usuario_Blacklist>().ToTable("usuario_blacklist");

            // CORREÇÃO: Removido o mapeamento duplicado que apontava Notificacao para Tipo_Evento
            modelBuilder.Entity<Notificacao>().ToTable("notificacao");
            modelBuilder.Entity<Tipo_Evento>().ToTable("tipo_evento");
            modelBuilder.Entity<AuditoriaLog>().ToTable("auditoria_log");

            // Configurações de Auditoria
            modelBuilder.Entity<AuditoriaLog>()
                .HasKey(a => a.Codigo_Log);

            // Configurações de Notificacao
            modelBuilder.Entity<Notificacao>()
                .HasKey(n => n.Codigo_Historico);

            modelBuilder.Entity<Notificacao>()
                .HasOne(n => n.UsuarioDestino)
                .WithMany()
                .HasForeignKey(n => n.Codigo_Usuario);

            modelBuilder.Entity<Notificacao>()
                .HasOne(ut => ut.ConfiguraOrigem)
                .WithMany(u => u.Notificacoes)
                .HasForeignKey(ut => ut.Codigo_Notificacao);

            // Relacionamentos UsuarioTelefone
            modelBuilder.Entity<UsuarioTelefone>()
                .HasOne(ut => ut.Usuario)
                .WithMany(u => u.UsuariosTelefones)
                .HasForeignKey(ut => ut.Codigo_Usuario);

            modelBuilder.Entity<UsuarioTelefone>()
                .HasOne(ut => ut.Telefone)
                .WithMany(t => t.UsuariosTelefones)
                .HasForeignKey(ut => ut.Codigo_Telefone);

            // Relacionamentos Blacklist
            modelBuilder.Entity<Usuario_Blacklist>()
                .HasOne(ub => ub.Blacklist)
                .WithMany(b => b.UsuariosBloqueados)
                .HasForeignKey(ub => ub.Codigo_BlackList)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Blacklist>()
                .HasOne(b => b.Agendamento)
                .WithMany()
                .HasForeignKey(b => b.Codigo_Agendamento);

            modelBuilder.Entity<Usuario_Blacklist>()
                .HasOne(ub => ub.Barbeiro)
                .WithMany()
                .HasForeignKey(ub => ub.Codigo_Usuario)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacionamentos Agendamento
            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Cliente)
                .WithMany()
                .HasForeignKey(a => a.Codigo_Cliente)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Barbeiro)
                .WithMany()
                .HasForeignKey(a => a.Codigo_Barbeiro)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacionamentos Agendamento_Servico
            modelBuilder.Entity<Agendamento_Servico>()
                .HasOne(a => a.Servico)
                .WithMany()
                .HasForeignKey(a => a.Codigo_Servico)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Agendamento_Servico>()
                .HasOne(a => a.Agendamento)
                .WithMany(a => a.Agendamento_Servicos)
                .HasForeignKey(a => a.Codigo_Agendamento)
                .OnDelete(DeleteBehavior.Restrict);
        }

        // IMPLEMENTAÇÃO DA AUDITORIA AUTOMÁTICA
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = new List<AuditoriaLog>();

            // Extrai o ID do usuário da requisição atual via token JWT
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? usuarioLogadoId = userIdClaim != null ? int.Parse(userIdClaim) : null;

            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is not AuditoriaLog &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .ToList();

            foreach (var entry in entries)
            {
                var auditEntry = new AuditoriaLog
                {
                    Entidade_Afetada = entry.Entity.GetType().Name,
                    Usuario_Acao = usuarioLogadoId,
                    Data_Hora_Acao = DateTime.UtcNow
                };

                // Identifica a chave primária
                var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                auditEntry.Registro_ID = primaryKey?.CurrentValue?.ToString() ?? "N/A";

                var oldValues = new Dictionary<string, object>();
                var newValues = new Dictionary<string, object>();

                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.Tipo_Acao = "INSERT";
                            if (property.CurrentValue != null)
                                newValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            auditEntry.Tipo_Acao = "DELETE";
                            if (property.OriginalValue != null)
                                oldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.Tipo_Acao = "UPDATE";
                                oldValues[propertyName] = property.OriginalValue;
                                newValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }

                auditEntry.Valores_Antigos = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null;
                auditEntry.Valores_Novos = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null;

                auditEntries.Add(auditEntry);
            }

            if (auditEntries.Any())
            {
                await AuditoriaLogs.AddRangeAsync(auditEntries, cancellationToken);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}