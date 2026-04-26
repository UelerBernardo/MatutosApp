using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MatutosInfraestrutura
{
    public class DBContext : DbContext
    {
        private IConfiguration _configuration;

        //public DbSet<Entidades.Users> Users { get; set; }
        //public DbSet<UsersDomain.Entidades.Agendamento> Agendamentos { get; set; }
        //public DbSet<UsersDomain.Entidades.Servicos> Servicos { get; set; }
        //public DbSet<UsersDomain.Entidades.Barbeiro> Barbeiros { get; set; }

        //public DbSet<UsersDomain.Entidades.AgendamentoServico> AgendamentoServicos { get; set; }
        public DBContext(IConfiguration configuration, DbContextOptions options) : base(options)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var typeDatabase = _configuration["TypeDatabase"];
            var connectionString = _configuration.GetConnectionString(typeDatabase);

            if (typeDatabase == "SqlServer")
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}
