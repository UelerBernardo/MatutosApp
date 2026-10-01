using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    public class BlacklistHorarios
    {
        public int Codigo_BlackList { get; set; }
        public DateTime Inicio_Bloqueio { get; set; }
        public DateTime Fim_Bloqueio { get; set; }
        public bool Ativo { get; set; }
    }
}
