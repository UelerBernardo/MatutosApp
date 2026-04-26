using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MatutosDomain
{
    [Table("Usuario_Telefone")]
    public class UsuarioTelefone
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ForeignKey("Usuario")]
        public int Codigo_Usuario { get; set; }

        [ForeignKey("Telefone")]
        public int Codigo_Telefone { get; set; }

        // Propriedades de Navegação com bloqueio de loop infinito
        [JsonIgnore]
        public virtual Usuario? Usuario { get; set; }

        [JsonIgnore]
        public virtual Telefone? Telefone { get; set; }

    }
}
