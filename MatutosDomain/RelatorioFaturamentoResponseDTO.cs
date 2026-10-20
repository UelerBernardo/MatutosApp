using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    public class RelatorioFaturamentoResponseDTO
    {
        public int IdAgendamento { get; set; }
        public int IdServico { get; set; }
        public string DescricaoServico { get; set; } = string.Empty;
        public int IdBarbeiro { get; set; }
        public string NomeBarbeiro { get; set; } = string.Empty;
        public int IdCliente { get; set; }
        public string NomeCliente { get; set; } = string.Empty;
        public decimal PrecoServico { get; set; }
        public DateTime DataHoraAgendamento { get; set; }
        public decimal TotalAgendamento { get; set; }
    }
}
