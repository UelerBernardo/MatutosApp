using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    [Table("servico_imagem")]
    public class Servico_Imagem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Codigo_Imagem { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string? Imagem { get; set; }

        [ForeignKey ("Servico")]
        [Column("Codigo_Servico")]
        public int Codigo_Servico { get; set; }

        public virtual Servico? Servico { get; set; }
    }
}
