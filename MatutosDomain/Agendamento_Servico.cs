using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    [Table ("Agendamento_Servico")]
    public class Agendamento_Servico
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Codigo_Agendamento_Servico { get; set; }
        [ForeignKey("Agendamento")]
        public int Codigo_Agendamento { get; set; }
        [ForeignKey("Servico")]
        public int Codigo_Servico { get; set; }
        public int Quantidade_Servico { get; set; }
        public Agendamento? Agendamento { get; set; }
        public Servico? Servico { get; set; }
        public decimal Valor_Total_Item { get; set; }
        public int Tempo_Servico_Item { get; set; }
    }
}
