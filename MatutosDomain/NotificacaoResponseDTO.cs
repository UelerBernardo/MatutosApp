using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    public class NotificacaoResponseDTO
    {
        public int? ConfiguraNotificacao { get; set; }
        public string? Mensagem { get; set; }
        public DateTime? DataDisparo { get; set; }
        public bool Lida { get; set; }
        public string? DescricaoRegra { get; set; }
        public int? TipoEvento { get; set; }
    }
}
