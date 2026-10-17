using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    public class FiltroLogDTO
    {
        public string? Entidade { get; set; }
        public TipoAcaoEnum? Acao { get; set; }
        public DateTime? Inicio { get; set; }
        public DateTime? Fim { get; set; }
        public string? UsuarioLog { get; set; }
    }
}
