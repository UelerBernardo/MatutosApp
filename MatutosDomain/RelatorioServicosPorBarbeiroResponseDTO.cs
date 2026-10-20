using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    public class RelatorioServicosPorBarbeiroResponseDTO
    {
        public int IdBarbeiro { get; set; }
        public string NomeBarbeiro { get; set; } = string.Empty;

        // Cuidado aqui: dependendo do banco de dados, SUM() pode retornar decimal ou int. 
        // Usar decimal previne erros de conversão.
        public decimal QuantidadeServicosPrestados { get; set; }
        public decimal ValorTotal { get; set; }
    }
}
