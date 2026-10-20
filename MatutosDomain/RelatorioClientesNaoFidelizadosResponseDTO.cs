using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    public class RelatorioClientesNaoFidelizadosResponseDTO
    {
        public int IdCliente { get; set; }
        public string NomeCliente { get; set; } = string.Empty;
        public DateTime DataUltimoAgendamento { get; set; }
        public int DiasSemAgendamento { get; set; }
    }
}
