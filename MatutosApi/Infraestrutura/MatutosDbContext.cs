using Microsoft.EntityFrameworkCore;
using MatutosDomain;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
            // Extrai o ID do usuário da requisição atual via token JWT
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? usuarioLogadoId = userIdClaim != null ? int.Parse(userIdClaim) : null;

            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is not AuditoriaLog &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .ToList();

            // Uma lista para manter as informações temporárias e a Entidade original na memória
            var auditInfos = new List<(EntityEntry Entry, AuditoriaLog Log, Dictionary<string, object> NewValues)>();

            foreach (var entry in entries)
            {
                var auditEntry = new AuditoriaLog
                {
                    Entidade_Afetada = entry.Entity.GetType().Name,
                    Usuario_Acao = usuarioLogadoId,
                    Data_Hora_Acao = DateTime.UtcNow
                };

                var oldValues = new Dictionary<string, object>();
                var newValues = new Dictionary<string, object>();

                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.Tipo_Acao = TipoAcaoEnum.Inserção;
                            if (property.CurrentValue != null)
                                newValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            auditEntry.Tipo_Acao = TipoAcaoEnum.Exclusão;
                            if (property.OriginalValue != null)
                                oldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.Tipo_Acao = TipoAcaoEnum.Alteração;
                                oldValues[propertyName] = property.OriginalValue;
                                newValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }

                // Para UPDATE e DELETE, o ID real já existe, então já podemos pegar.
                // Para o INSERT, deixaremos vazio por enquanto.
                if (entry.State != EntityState.Added)
                {
                    var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                    auditEntry.Registro_ID = primaryKey?.CurrentValue?.ToString() ?? "N/A";
                }

                auditEntry.Valores_Antigos = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null;

                // Guardamos as informações para atualizar o ID mais tarde.
                // NOTA: Não serializamos Valores_Novos ainda, para podermos colocar o ID real dentro do JSON se desejado.
                auditInfos.Add((entry, auditEntry, newValues));
            }

            // 1º SALVAMENTO: Envia as entidades originais para o banco.
            // É AQUI que o banco gera os IDs reais (como o ID do Servico) e o EF Core atualiza o objeto na memória.
            var result = await base.SaveChangesAsync(cancellationToken);

            // Agora passamos pelos registros de auditoria preparados e atualizamos os INSERTS
            foreach (var info in auditInfos)
            {
                // Se foi uma inserção, agora a entidade possui o ID gerado pelo banco
                if (info.Log.Tipo_Acao == TipoAcaoEnum.Inserção)
                {
                    var primaryKey = info.Entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                    info.Log.Registro_ID = primaryKey?.CurrentValue?.ToString() ?? "N/A";

                    // (Opcional) Se você quiser que o ID real também apareça dentro do JSON de "Valores_Novos"
                    if (primaryKey != null)
                    {
                        info.NewValues[primaryKey.Metadata.Name] = primaryKey.CurrentValue;
                    }
                }

                // Serializamos os novos valores apenas agora, garantindo que contenham o ID real
                info.Log.Valores_Novos = info.NewValues.Count > 0 ? JsonSerializer.Serialize(info.NewValues) : null;

                // Adicionamos o log de auditoria ao contexto
                AuditoriaLogs.Add(info.Log);
            }

            if (auditInfos.Any())
            {
                // 2º SALVAMENTO: Salva exclusivamente a tabela de auditoria agora com os IDs corretos.
                await base.SaveChangesAsync(cancellationToken);
            }

            return result;
        }
    }
}