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
    [Table("Telefone")]
    public class Telefone
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Codigo_Telefone { get; set; }
        public string? Numero_Telefone { get; set; }
        public string? DDD { get; set; }
        public bool Principal { get; set; }

        [JsonIgnore]
        public virtual ICollection<UsuarioTelefone> UsuariosTelefones { get; set; } = new List<UsuarioTelefone>();
    }
}
