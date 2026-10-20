using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    public class FiltroFaturamentoDTO
    {
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string? DescricaoServico { get; set; }
        public string? NomeCliente { get; set; }
    }
}
