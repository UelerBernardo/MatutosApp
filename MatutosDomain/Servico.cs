using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    [Table("Servico")]
    public class Servico
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Codigo_Servico { get; set; }
        public string? Descricao { get; set; }
        public string? Duracao { get; set; }
        public decimal Preco { get; set; }
        public int Tempo_Servico { get; set; }
        public bool Ativo { get; set; }

        public virtual ICollection<Servico_Imagem> Imagens { get; set; } = new List<Servico_Imagem>();
    }
}
