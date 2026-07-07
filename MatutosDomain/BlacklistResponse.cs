using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    public class BlacklistResponse
    {
        public int Codigo_BlackList { get; set; }
        public DateTime Inicio_Bloqueio { get; set; }
        public DateTime Fim_Bloqueio { get; set; }
        public bool Ativo { get; set; }

        // 👉 O SEGREDO: Aqui nós dizemos ao MAUI que vai chegar apenas uma lista de números (IDs)!
        public List<int> UsuariosBloqueados { get; set; }
        public string? Detalhes { get; set; }
        public int? Codigo_Agendamento { get; set; }
    }
}
