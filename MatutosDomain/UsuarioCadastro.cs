using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    public class UsuarioCadastro
    {
        public string? Nome { get; set; } 
        public string? Email { get; set; }
        public string? Senha { get; set; }
        [NotMapped]
        public UsuarioTipo TipoSelecionado { get; set; }
    }
}
