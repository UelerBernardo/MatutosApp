using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    [Table("notificacao")]
    public class Notificacao
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Codigo_Historico { get; set; }
        public int Codigo_Notificacao { get; set; }
        public int Codigo_Usuario { get; set; }
        public int? Codigo_Agendamento { get; set; }
        public string MensagemEnviada { get; set; } = string.Empty;
        public DateTime DataDisparo { get; set; }
        public bool Lida { get; set; } = false;

        // Propriedades de Navegação
        public Configura_Notificacao? ConfiguraOrigem { get; set; }
        [ForeignKey("Codigo_Usuario")]
        public Usuario? UsuarioDestino { get; set; }
        // public Agendamento? AgendamentoRelacionado { get; set; } 

        // Coloque isso dentro da classe ConfiguraNotificacao
    }
}
