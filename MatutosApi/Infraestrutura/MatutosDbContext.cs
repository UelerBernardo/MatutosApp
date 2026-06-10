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