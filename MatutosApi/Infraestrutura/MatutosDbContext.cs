using Microsoft.EntityFrameworkCore;
using MatutosDomain;
using Org.BouncyCastle.Asn1;

namespace MatutosApi.Infraestrutura 
{
    public class MatutosDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Barbeiro> Barbeiros {  get; set; }
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


        public MatutosDbContext(DbContextOptions<MatutosDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Tabelas do banco de dados
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
            modelBuilder.Entity<Notificacao>().ToTable("notificacao");
            modelBuilder.Entity<Notificacao>().ToTable("tipo_evento");





            //modelBuilder.Entity<Configura_Notificacao>()
            //    .HasOne(ut => ut.TipoEventoRelacionado)
            //    .WithMany(u => u.Codigo_Tipo)
            //    .HasForeignKey(ut => ut.Codigo_Notificacao);

            modelBuilder.Entity<Tipo_Evento>()
                 .ToTable("Tipo_Evento");

            modelBuilder.Entity<Notificacao>()
                .HasOne(n => n.UsuarioDestino) // A propriedade de navegação
                .WithMany() // Ou com a coleção correspondente na classe Usuario
                .HasForeignKey(n => n.Codigo_Usuario); // 👉 A coluna real do MariaDB!

        // O ERRO PROVAVELMENTE ESTÁ AQUI: 
        // Veja se a Notificacao não está apontando para "Tipo_Evento" também.
        // O correto é:
        modelBuilder.Entity<Notificacao>()
                .ToTable("Notificacao");

            // 👉 A linha que salva o dia avisando qual é a chave primária:
            modelBuilder.Entity<Notificacao>()
                .HasKey(n => n.Codigo_Historico);

            // O seu relacionamento que você já tinha feito continua logo abaixo:
            modelBuilder.Entity<Notificacao>()
                .HasOne(ut => ut.ConfiguraOrigem)
                .WithMany(u => u.Notificacoes)
                .HasForeignKey(ut => ut.Codigo_Notificacao);

            // Ensina o caminho do Usuário para a tabela ponte
            modelBuilder.Entity<UsuarioTelefone>()
                .HasOne(ut => ut.Usuario)
                .WithMany(u => u.UsuariosTelefones) 
                .HasForeignKey(ut => ut.Codigo_Usuario);

            // Ensina o caminho do Telefone para a tabela ponte
            modelBuilder.Entity<UsuarioTelefone>()
                .HasOne(ut => ut.Telefone)
                .WithMany(t => t.UsuariosTelefones)
                .HasForeignKey(ut => ut.Codigo_Telefone); // CORRIGIDO: Removida a duplicação

            modelBuilder.Entity<Usuario_Blacklist>()
                .HasOne(ub => ub.Blacklist)
                .WithMany(b => b.UsuariosBloqueados)
                .HasForeignKey(ub => ub.Codigo_BlackList)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Blacklist>()
                .HasOne(b => b.Agendamento)
                .WithMany() // ou a propriedade de coleção lá na classe Agendamento, se houver
                .HasForeignKey(b => b.Codigo_Agendamento);

            modelBuilder.Entity<Usuario_Blacklist>()
                .HasOne(ub => ub.Barbeiro)
                .WithMany()
                .HasForeignKey(ub => ub.Codigo_Usuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Cliente)        // O Agendamento tem um Cliente
                .WithMany()                    // O Cliente pode ter muitos agendamentos
                .HasForeignKey(a => a.Codigo_Cliente) // A coluna de ligação é ESTRITAMENTE essa
                .OnDelete(DeleteBehavior.Restrict);   // Evita que apagar o cliente apague a tabela inteira sem querer

            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Barbeiro)       // O Agendamento tem um Barbeiro
                .WithMany()                    // O Barbeiro pode ter muitos agendamentos
                .HasForeignKey(a => a.Codigo_Barbeiro) // A coluna de ligação é ESTRITAMENTE essa
                .OnDelete(DeleteBehavior.Restrict);

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
    }
}