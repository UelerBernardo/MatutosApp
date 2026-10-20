using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    public class FiltroServicosPorBarbeiroDTO
    {
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string? NomeBarbeiro { get; set; }
    }
}
