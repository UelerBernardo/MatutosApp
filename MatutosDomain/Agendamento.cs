using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MatutosDomain
{
    [Table ("Agendamento")]
    public class Agendamento
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Codigo_Agendamento { get; set;}
        public DateTime? Data_Agendamento { get; set; }
        public DateTime? Data_Fim_Agendamento { get; set;}
        public bool Ativo { get; set;}
        public decimal Valor_Total_Agendamento { get; set; }
        [ForeignKey ("Cliente")]
        [Column("Codigo_Cliente")]
        public int Codigo_Cliente { get; set; }
        [ForeignKey ("Barbeiro")]
        [Column("Codigo_Barbeiro")]
        public int Codigo_Barbeiro { get; set; }
        [JsonIgnore]
        public Cliente? Cliente { get; set; }
        [JsonIgnore]
        public Barbeiro? Barbeiro { get; set; }
        public AgendamentoSituacao Codigo_Situacao_Agendamento { get; set; }

        public ICollection<Agendamento_Servico> Agendamento_Servicos { get; set; } = new List<Agendamento_Servico>();

    }
}
