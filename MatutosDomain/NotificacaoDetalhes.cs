using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    public class NotificacaoDetalhes
    {
        public int Codigo_Historico { get; set; }
        public int Codigo_Notificacao { get; set; }
        public int Codigo_Usuario { get; set; }
        public string? MensagemEnviada { get; set; }
        public DateTime DataDisparo { get; set; }
        public bool Lida { get; set; }
        public string? NomeCliente { get; set; }
        public string? Descricao { get; set; }
    }
}
