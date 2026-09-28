using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    public class MeusServicos
    {
        public int Codigo_Agendamento { get; set; }
        public DateTime? Data_Agendamento { get; set; }
        public DateTime? Data_Fim_Agendamento { get; set; }
        public bool Ativo { get; set; }
        public decimal Valor_Total_Agendamento { get; set; }
        public int Codigo_Cliente { get; set; }
        public int Codigo_Barbeiro { get; set; }
        public AgendamentoSituacao Codigo_Situacao_Agendamento { get; set; }
        public string? Nome_Cliente { get; set; }
    }
}
