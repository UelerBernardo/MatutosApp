using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    [Table("blacklist")]
    public class Blacklist
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Codigo_BlackList { get; set; }
        public bool Ativo { get; set; }
        public DateTime Inicio_Bloqueio { get; set; }
        public DateTime Fim_Bloqueio { get; set; }
        public string? Detalhes { get; set; }
        public ICollection<Usuario_Blacklist>? UsuariosBloqueados { get; set;}
        [ForeignKey("agendamento")]
        public int Codigo_Agendamento { get; set; }
        public Agendamento? Agendamento { get; set; }
    }
}
