using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    [Table("configura_notificacao")]
    public class Configura_Notificacao
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Codigo_Notificacao { get; set; }
        public bool Ativo { get; set; } = true;
        public int Codigo_Tipo { get; set; }
        public string? Descricao { get; set; }
        public string? Mensagem { get; set; }
        public int? Valor { get; set; }
        public UnidadeTempoEnum? UnidadeTempo { get; set; }
        [ForeignKey("Codigo_Tipo")]
        public Tipo_Evento? TipoEventoRelacionado { get; set; }

        public ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();
    }
}
