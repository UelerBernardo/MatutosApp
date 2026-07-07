using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    [Table("usuario_blacklist")]
    public class Usuario_Blacklist
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Codigo_Usuario_BlackList { get; set; }
        [ForeignKey("barbeiro")]
        public int Codigo_Usuario { get; set; }
        [ForeignKey("blacklist")]
        public int Codigo_BlackList { get; set; }
        public Barbeiro? Barbeiro { get; set; }
        public Blacklist? Blacklist { get; set; }


    }
}
